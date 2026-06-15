#!/usr/bin/env python3
"""
Generate review.json for inline PR review by calling OpenAI directly.

CI uses this instead of the OpenCode CLI because opencode run is unreliable in
headless Actions (agent quirks, -f/-- parsing, NDJSON extraction). Same rules,
same context files, same review.json schema consumed by post-inline-review.py.

Set REVIEW_ENGINE=opencode to try the CLI first (local debugging only).
"""
from __future__ import annotations

import json
import os
import re
import sys
import urllib.error
import urllib.request
from pathlib import Path

MAX_DIFF_CHARS = 120_000
MAX_FILE_CHARS = 30_000
MAX_TOTAL_CHARS = 180_000
MODEL = os.environ.get("OPENAI_REVIEW_MODEL", "gpt-4o")


def read_text(path: Path, limit: int | None = None) -> str:
    if not path.exists():
        return ""
    text = path.read_text(encoding="utf-8", errors="replace")
    if limit and len(text) > limit:
        return text[:limit] + f"\n\n...(truncated, {len(text)} chars total)"
    return text


def load_optional(repo: Path, rel: str, limit: int | None = None) -> str:
    return read_text(repo / rel, limit)


def bundle_context(repo: Path) -> str:
    ctx = repo / ".review-context"
    parts: list[str] = []

    parts.append("# INDEX\n" + read_text(ctx / "INDEX.md"))
    parts.append("# VALID_LINES (GitHub inline comment lines — use ONLY these)\n" + read_text(ctx / "valid-lines.json"))
    parts.append("# DIFF\n" + read_text(ctx / "diff.patch", MAX_DIFF_CHARS))

    for numbered in sorted(ctx.glob("*.txt")):
        parts.append(f"# NUMBERED FILE: {numbered.name}\n" + read_text(numbered, MAX_FILE_CHARS))

    context_dir = ctx / "context"
    if context_dir.is_dir():
        for f in sorted(context_dir.rglob("*")):
            if f.is_file():
                parts.append(f"# CONTEXT: {f.relative_to(context_dir)}\n" + read_text(f, MAX_FILE_CHARS))

    related_dir = ctx / "related"
    if related_dir.is_dir():
        for f in sorted(related_dir.rglob("*.txt")):
            parts.append(f"# RELATED: {f.name}\n" + read_text(f, MAX_FILE_CHARS))

    # Auth comparison targets (often cited in smoke-test PR)
    for extra in (
        "WebApplication1/ApiKeyAuth.cs",
        "WebApplication1/Controllers/RefundController.cs",
        "AI_REVIEW.md",
        "AGENTS.md",
        "docs/REVIEW_PROJECT_CONTEXT.md",
    ):
        text = load_optional(repo, extra, MAX_FILE_CHARS)
        if text:
            parts.append(f"# REFERENCE: {extra}\n{text}")

    bundled = "\n\n---\n\n".join(p for p in parts if p.strip())
    if len(bundled) > MAX_TOTAL_CHARS:
        bundled = bundled[:MAX_TOTAL_CHARS] + "\n\n...(context truncated)"
    return bundled


def system_prompt() -> str:
    return read_text(Path(".github/prompts/inline-review-prompt.md")) or (
        "You are a code reviewer. Output JSON with summary and comments array only."
    )


def call_openai(api_key: str, system: str, user: str) -> dict:
    payload = {
        "model": MODEL,
        "temperature": 0.2,
        "response_format": {"type": "json_object"},
        "messages": [
            {"role": "system", "content": system},
            {"role": "user", "content": user},
        ],
    }
    req = urllib.request.Request(
        "https://api.openai.com/v1/chat/completions",
        data=json.dumps(payload).encode("utf-8"),
        headers={
            "Authorization": f"Bearer {api_key}",
            "Content-Type": "application/json",
        },
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=180) as resp:
            body = json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"OpenAI API failed ({e.code}): {err}") from e

    content = body["choices"][0]["message"]["content"]
    data = json.loads(content)
    if not isinstance(data, dict):
        raise RuntimeError("OpenAI response was not a JSON object")
    return data


def validate_review(data: dict, repo: Path) -> dict:
    if "summary" not in data:
        data["summary"] = "AI code review"
    if not isinstance(data.get("comments"), list):
        data["comments"] = []

    valid_lines_path = repo / ".review-context" / "valid-lines.json"
    valid_lines: dict[str, list[int]] = {}
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
        "Review the pull request using the bundled context below.\n"
        "Return ONE JSON object with keys `summary` (string) and `comments` (array).\n"
        "Each comment needs: path, line, side (RIGHT), severity, body.\n"
        "Use line numbers from NUMBERED FILE sections; they must appear in VALID_LINES.\n"
        "Focus on changed files in INDEX. Max 15 comments.\n\n"
        + bundled
    )

    print(f"Calling OpenAI ({MODEL}) with {len(user_message)} chars of context...", file=sys.stderr)
    data = call_openai(api_key, system_prompt(), user_message)
    data = validate_review(data, repo)
    out.write_text(json.dumps(data, indent=2), encoding="utf-8")
    print(f"Wrote {out} with {len(data['comments'])} comment(s)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
