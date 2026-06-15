#!/usr/bin/env python3
"""
Generate review.json for inline PR review by calling OpenAI directly.

CI uses this instead of the OpenCode CLI because opencode run is unreliable in
headless Actions. Keeps bundled context small to fit org TPM limits (~30k tokens).
"""
from __future__ import annotations

import json
import os
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

# ~12k tokens user + ~2k system + ~2k output ≈ under 30k TPM tier limits
MAX_USER_CHARS = int(os.environ.get("OPENAI_REVIEW_MAX_CHARS", "48000"))
MAX_DIFF_CHARS = 20_000
MAX_NUMBERED_FILE_CHARS = 6_000
MAX_REFERENCE_CHARS = 2_500
MODEL = os.environ.get("OPENAI_REVIEW_MODEL", "gpt-4o")
RETRY_ATTEMPTS = 3
RETRY_DELAY_SEC = 30

COMPACT_SYSTEM_PROMPT = """You are an automated code reviewer for a .NET 7 CQRS e-commerce API.

Review ONLY changed files listed in INDEX. Compare against auth/validation patterns in REFERENCE snippets.

Priorities: security → correctness → domain integrity → performance → resilience.

Flag:
- Missing ApiKeyAuth / X-Api-Key on sensitive endpoints (see ApiKeyAuth.cs + RefundController pattern)
- Secrets/PII in logs or API responses (connection strings, amounts, tax IDs)
- Missing FluentValidation on commands; missing SaveChangesAsync after mutations
- SQL injection, unbounded lists, sync-over-async

Output ONE JSON object only:
{"summary":"2-4 sentences with risk Low/Medium/High","comments":[{"path":"...","line":N,"side":"RIGHT","severity":"blocker|high|medium|low","body":"..."}]}

Rules:
- Max 15 comments; high-confidence only
- path = changed file from INDEX only
- line MUST be from VALID_LINES for that path
- side = RIGHT
- If no issues: {"summary":"No high-confidence issues.","comments":[]}"""


def read_text(path: Path, limit: int | None = None) -> str:
    if not path.exists():
        return ""
    text = path.read_text(encoding="utf-8", errors="replace")
    if limit and len(text) > limit:
        return text[:limit] + f"\n...(truncated, {len(text)} chars total)"
    return text


def add_part(parts: list[str], used: list[int], budget: int, header: str, body: str) -> None:
    body = body.strip()
    if not body:
        return
    block = f"{header}{body}"
    remaining = budget - used[0]
    if remaining <= 200:
        return
    if len(block) > remaining:
        block = block[:remaining] + "\n...(truncated for token budget)"
    parts.append(block)
    used[0] += len(block)


def bundle_context(repo: Path) -> str:
    ctx = repo / ".review-context"
    parts: list[str] = []
    used = [0]
    budget = MAX_USER_CHARS

    add_part(parts, used, budget, "# INDEX\n", read_text(ctx / "INDEX.md", 4_000))
    add_part(
        parts,
        used,
        budget,
        "# VALID_LINES (inline comment lines — use ONLY these)\n",
        read_text(ctx / "valid-lines.json", 8_000),
    )
    add_part(parts, used, budget, "# DIFF\n", read_text(ctx / "diff.patch", MAX_DIFF_CHARS))

    # Changed files with line numbers (highest value for inline comments)
    for numbered in sorted(ctx.glob("*.txt")):
        add_part(
            parts,
            used,
            budget,
            f"# NUMBERED: {numbered.name}\n",
            read_text(numbered, MAX_NUMBERED_FILE_CHARS),
        )

    # Small auth comparison snippets only (not full duplicate context bundle)
    for extra in (
        "WebApplication1/ApiKeyAuth.cs",
        "WebApplication1/Controllers/RefundController.cs",
    ):
        text = read_text(repo / extra, MAX_REFERENCE_CHARS)
        if text:
            add_part(parts, used, budget, f"# REFERENCE: {extra}\n", text)

    return "\n\n---\n\n".join(parts)


def call_openai(api_key: str, system: str, user: str) -> dict:
    payload = {
        "model": MODEL,
        "temperature": 0.2,
        "max_tokens": 4096,
        "response_format": {"type": "json_object"},
        "messages": [
            {"role": "system", "content": system},
            {"role": "user", "content": user},
        ],
    }
    data = json.dumps(payload).encode("utf-8")
    last_error: Exception | None = None

    for attempt in range(1, RETRY_ATTEMPTS + 1):
        req = urllib.request.Request(
            "https://api.openai.com/v1/chat/completions",
            data=data,
            headers={
                "Authorization": f"Bearer {api_key}",
                "Content-Type": "application/json",
            },
            method="POST",
        )
        try:
            with urllib.request.urlopen(req, timeout=180) as resp:
                body = json.loads(resp.read().decode("utf-8"))
            content = body["choices"][0]["message"]["content"]
            parsed = json.loads(content)
            if not isinstance(parsed, dict):
                raise RuntimeError("OpenAI response was not a JSON object")
            return parsed
        except urllib.error.HTTPError as e:
            err = e.read().decode("utf-8", errors="replace")
            last_error = RuntimeError(f"OpenAI API failed ({e.code}): {err}")
            if e.code == 429 and attempt < RETRY_ATTEMPTS:
                wait = RETRY_DELAY_SEC * attempt
                print(f"Rate limited (429), retry {attempt}/{RETRY_ATTEMPTS} in {wait}s...", file=sys.stderr)
                time.sleep(wait)
                continue
            raise last_error from e

    raise last_error or RuntimeError("OpenAI API call failed")


def validate_review(data: dict, repo: Path) -> dict:
    if "summary" not in data:
        data["summary"] = "AI code review"
    if not isinstance(data.get("comments"), list):
        data["comments"] = []

    valid_lines: dict[str, list[int]] = {}
    valid_lines_path = repo / ".review-context" / "valid-lines.json"
    if valid_lines_path.exists():
        try:
            valid_lines = json.loads(valid_lines_path.read_text(encoding="utf-8"))
        except json.JSONDecodeError:
            pass

    cleaned = []
    for c in data["comments"][:15]:
        if not isinstance(c, dict):
            continue
        path = (c.get("path") or "").strip().lstrip("./")
        body = (c.get("body") or c.get("message") or "").strip()
        if not path or not body:
            continue
        try:
            line = int(c.get("line"))
        except (TypeError, ValueError):
            continue
        allowed = valid_lines.get(path, [])
        if allowed and line not in allowed:
            line = min(allowed, key=lambda x: abs(x - line))
        cleaned.append(
            {
                "path": path,
                "line": line,
                "side": c.get("side") or "RIGHT",
                "severity": c.get("severity") or "medium",
                "body": body,
            }
        )
    data["comments"] = cleaned
    return data


def main() -> int:
    repo = Path(os.environ.get("GITHUB_WORKSPACE", ".")).resolve()
    out = Path(sys.argv[1] if len(sys.argv) > 1 else "review.json")
    api_key = os.environ.get("OPENAI_API_KEY", "").strip()
    if not api_key:
        raise SystemExit("ERROR: OPENAI_API_KEY is not set")

    bundled = bundle_context(repo)
    if not bundled.strip():
        raise SystemExit("ERROR: .review-context is empty — run build-review-context.sh first")

    user_message = (
        "Review this PR. Return JSON with summary and comments only.\n\n" + bundled
    )
    system = COMPACT_SYSTEM_PROMPT
    total_chars = len(system) + len(user_message)
    est_tokens = total_chars // 4
    print(
        f"Calling OpenAI ({MODEL}): system={len(system)} + user={len(user_message)} "
        f"chars (~{est_tokens} tokens est.)",
        file=sys.stderr,
    )

    if est_tokens > 28_000:
        print("WARNING: estimated tokens near limit; consider raising OPENAI tier or lowering OPENAI_REVIEW_MAX_CHARS", file=sys.stderr)

    data = call_openai(api_key, system, user_message)
    data = validate_review(data, repo)
    out.write_text(json.dumps(data, indent=2), encoding="utf-8")
    print(f"Wrote {out} with {len(data['comments'])} comment(s)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
