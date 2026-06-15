#!/usr/bin/env python3
"""
Post inline PR review comments from review.json using GitHub Pull Request Reviews API.
https://docs.github.com/en/rest/pulls/reviews#create-a-review-for-a-pull-request

GitHub only accepts inline comments on lines that appear in the PR diff (RIGHT side).
OpenCode may cite valid file line numbers outside diff hunks — we validate against each
file's patch and snap to the nearest in-diff line when close enough.
"""
from __future__ import annotations

import json
import os
import re
import sys
import urllib.error
import urllib.request

MAX_INLINE = 50
MAX_BODY = 65000
MAX_LINE_SNAP_DISTANCE = 20


def api(method: str, url: str, token: str, payload: dict | None = None) -> dict:
    data = None
    headers = {
        "Authorization": f"Bearer {token}",
        "Accept": "application/vnd.github+json",
        "X-GitHub-Api-Version": "2022-11-28",
    }
    if payload is not None:
        data = json.dumps(payload).encode("utf-8")
        headers["Content-Type"] = "application/json"
    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            body = resp.read().decode("utf-8")
            return json.loads(body) if body else {}
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"GitHub API {method} {url} failed ({e.code}): {err}") from e


def load_review(path: str) -> dict:
    raw = open(path, encoding="utf-8").read().strip()
    if not raw:
        raise ValueError(f"{path} is empty")
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        match = re.search(r"```(?:json)?\s*(\{.*\})\s*```", raw, re.DOTALL)
        if match:
            return json.loads(match.group(1))
        start, end = raw.find("{"), raw.rfind("}")
        if start >= 0 and end > start:
            return json.loads(raw[start : end + 1])
        raise


def fetch_pr_files(owner: str, repo: str, pr: int, token: str) -> dict[str, dict]:
    """Return filename -> GitHub pulls/files entry (includes optional patch)."""
    files: dict[str, dict] = {}
    page = 1
    while True:
        url = f"https://api.github.com/repos/{owner}/{repo}/pulls/{pr}/files?per_page=100&page={page}"
        batch = api("GET", url, token)
        if not batch:
            break
        for entry in batch:
            files[entry["filename"]] = entry
        if len(batch) < 100:
            break
        page += 1
    return files


def parse_patch_right_lines(patch: str) -> set[int]:
    """Line numbers on the RIGHT (new) side that appear in the unified diff."""
    valid: set[int] = set()
    new_line = 0
    in_hunk = False

    for line in patch.splitlines():
        if line.startswith("@@"):
            match = re.search(r"\+(\d+)(?:,(\d+))?", line)
            if match:
                new_line = int(match.group(1))
                in_hunk = True
            continue
        if not in_hunk:
            continue
        if line.startswith("+++") or line.startswith("---"):
            continue
        if line.startswith("+"):
            valid.add(new_line)
            new_line += 1
        elif line.startswith("-"):
            continue
        elif line.startswith(" ") or line == "":
            valid.add(new_line)
            new_line += 1
        elif line.startswith("\\"):
            continue

    return valid


def resolve_review_line(
    path: str,
    requested: int,
    pr_files: dict[str, dict],
) -> tuple[int | None, str | None]:
    """
    Map a requested file line to a line GitHub can attach in the PR diff.
    Returns (resolved_line, skip_reason).
    """
    entry = pr_files.get(path)
    if entry is None:
        return None, "not in PR file list"

    patch = entry.get("patch")
    if not patch:
        # Large/binary files have no patch — inline comments are not supported.
        return None, "file has no diff patch (too large or binary)"

    valid = parse_patch_right_lines(patch)
    if not valid:
        return None, "could not parse diff hunks"

    if requested in valid:
        return requested, None

    nearby = [line for line in valid if abs(line - requested) <= MAX_LINE_SNAP_DISTANCE]
    if nearby:
        snapped = min(nearby, key=lambda line: abs(line - requested))
        return snapped, None

    lo, hi = min(valid), max(valid)
    return None, f"line {requested} not in PR diff (valid in-diff range: {lo}-{hi})"


def format_comment(body: str, severity: str | None, requested_line: int | None = None, resolved_line: int | None = None) -> str:
    sev = (severity or "medium").lower()
    badge = {"blocker": "🔴 **Blocker**", "high": "🟠 **High**", "medium": "🟡 **Medium**", "low": "⚪ **Low**"}.get(
        sev, f"**{sev.title()}**"
    )
    text = body.strip()
    if not text.startswith("**"):
        text = f"{badge}\n\n{text}"
    if requested_line is not None and resolved_line is not None and requested_line != resolved_line:
        text = f"{text}\n\n_(Requested line {requested_line}; anchored to in-diff line {resolved_line}.)_"
    return text[:MAX_BODY]


def build_inline_comments(
    raw_comments: list,
    pr_files: dict[str, dict],
) -> tuple[list[dict], list[str]]:
    inline: list[dict] = []
    skipped: list[str] = []

    for i, comment in enumerate(raw_comments[:MAX_INLINE]):
        path = (comment.get("path") or "").strip().lstrip("./").lstrip("/")
        body = comment.get("body") or comment.get("message") or ""
        if not path or not body:
            skipped.append(f"#{i + 1}: missing path or body")
            continue

        try:
            requested_line = int(comment.get("line"))
        except (TypeError, ValueError):
            skipped.append(f"{path}: invalid line {comment.get('line')!r}")
            continue
        if requested_line < 1:
            skipped.append(f"{path}:{requested_line}: line must be >= 1")
            continue

        resolved, reason = resolve_review_line(path, requested_line, pr_files)
        if resolved is None:
            skipped.append(f"{path}:{requested_line}: {reason}")
            continue

        inline.append(
            {
                "path": path,
                "line": resolved,
                "side": comment.get("side") or "RIGHT",
                "body": format_comment(
                    body,
                    comment.get("severity"),
                    requested_line=requested_line,
                    resolved_line=resolved,
                ),
            }
        )

    return inline, skipped


def post_review(
    owner: str,
    repo: str,
    pr: int,
    token: str,
    sha: str,
    summary: str,
    inline: list[dict],
) -> dict:
    url = f"https://api.github.com/repos/{owner}/{repo}/pulls/{pr}/reviews"
    payload: dict = {
        "commit_id": sha,
        "body": summary,
        "event": "COMMENT",
    }
    if inline:
        payload["comments"] = inline
    return api("POST", url, token, payload)


def post_comments_individually(
    owner: str,
    repo: str,
    pr: int,
    token: str,
    sha: str,
    inline: list[dict],
) -> tuple[int, list[str]]:
    """Fallback: one review comment per request (partial success if some fail)."""
    url = f"https://api.github.com/repos/{owner}/{repo}/pulls/{pr}/comments"
    posted = 0
    errors: list[str] = []

    for comment in inline:
        payload = {
            "commit_id": sha,
            "path": comment["path"],
            "line": comment["line"],
            "side": comment.get("side", "RIGHT"),
            "body": comment["body"],
        }
        try:
            api("POST", url, token, payload)
            posted += 1
        except RuntimeError as exc:
            errors.append(f"{comment['path']}:{comment['line']}: {exc}")

    return posted, errors


def main() -> int:
    token = os.environ["GITHUB_TOKEN"]
    repo_full = os.environ["GITHUB_REPOSITORY"]
    pr = int(os.environ["PR_NUMBER"])
    sha = os.environ["PR_HEAD_SHA"]
    review_path = os.environ.get("REVIEW_JSON", "review.json")

    owner, repo = repo_full.split("/", 1)
    data = load_review(review_path)
    summary = (data.get("summary") or "AI code review (OpenCode)").strip()[:MAX_BODY]
    raw_comments = data.get("comments") or []

    pr_files = fetch_pr_files(owner, repo, pr, token)
    inline, skipped = build_inline_comments(raw_comments, pr_files)

    if skipped:
        summary += "\n\n---\n**Notes (could not attach inline):**\n" + "\n".join(f"- {s}" for s in skipped[:20])

    try:
        result = post_review(owner, repo, pr, token, sha, summary, inline)
        print(f"Posted review id={result.get('id')} with {len(inline)} inline comment(s).")
        if result.get("html_url"):
            print(result["html_url"])
        return 0
    except RuntimeError as batch_error:
        if not inline:
            raise

        print(f"Batch review failed: {batch_error}", file=sys.stderr)
        print("Retrying inline comments individually...", file=sys.stderr)

        posted, errors = post_comments_individually(owner, repo, pr, token, sha, inline)
        if errors:
            summary += "\n\n---\n**Inline post failures:**\n" + "\n".join(f"- {e}" for e in errors[:20])

        # Post summary-only review so the run still leaves a visible review thread.
        result = post_review(owner, repo, pr, token, sha, summary, [])
        print(f"Posted summary review id={result.get('id')} after {posted}/{len(inline)} inline comment(s).")
        if result.get("html_url"):
            print(result["html_url"])
        return 0 if posted > 0 or not errors else 1


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        raise SystemExit(1)
