#!/usr/bin/env python3
"""
Post inline PR review comments from review.json using GitHub Pull Request Reviews API.
https://docs.github.com/en/rest/pulls/reviews#create-a-review-for-a-pull-request
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


def fetch_changed_paths(owner: str, repo: str, pr: int, token: str) -> set[str]:
    paths: set[str] = set()
    page = 1
    while True:
        url = f"https://api.github.com/repos/{owner}/{repo}/pulls/{pr}/files?per_page=100&page={page}"
        files = api("GET", url, token)
        if not files:
            break
        for f in files:
            paths.add(f["filename"])
        if len(files) < 100:
            break
        page += 1
    return paths


def format_comment(body: str, severity: str | None) -> str:
    sev = (severity or "medium").lower()
    badge = {"blocker": "🔴 **Blocker**", "high": "🟠 **High**", "medium": "🟡 **Medium**", "low": "⚪ **Low**"}.get(
        sev, f"**{sev.title()}**"
    )
    text = body.strip()
    if not text.startswith("**"):
        text = f"{badge}\n\n{text}"
    return text[:MAX_BODY]


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

    changed = fetch_changed_paths(owner, repo, pr, token)
    inline: list[dict] = []
    skipped: list[str] = []

    for i, c in enumerate(raw_comments[:MAX_INLINE]):
        path = (c.get("path") or "").strip().lstrip("/")
        line = c.get("line")
        body = c.get("body") or c.get("message") or ""
        if not path or not body:
            skipped.append(f"#{i + 1}: missing path or body")
            continue
        if changed and path not in changed:
            skipped.append(f"{path}: not in PR file list")
            continue
        try:
            line_int = int(line)
        except (TypeError, ValueError):
            skipped.append(f"{path}: invalid line {line!r}")
            continue
        if line_int < 1:
            skipped.append(f"{path}:{line_int}: line must be >= 1")
            continue

        inline.append(
            {
                "path": path,
                "line": line_int,
                "side": c.get("side", "RIGHT"),
                "body": format_comment(body, c.get("severity")),
            }
        )

    if skipped:
        summary += "\n\n---\n**Notes (could not attach inline):**\n" + "\n".join(f"- {s}" for s in skipped[:20])

    payload = {
        "commit_id": sha,
        "body": summary,
        "event": "COMMENT",
        "comments": inline,
    }

    url = f"https://api.github.com/repos/{owner}/{repo}/pulls/{pr}/reviews"
    result = api("POST", url, token, payload)

    print(f"Posted review id={result.get('id')} with {len(inline)} inline comment(s).")
    if result.get("html_url"):
        print(result["html_url"])
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        raise SystemExit(1)
