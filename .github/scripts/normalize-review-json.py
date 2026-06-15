#!/usr/bin/env python3
"""Extract review.json from OpenCode CLI output.

OpenCode `run --format json` writes NDJSON events (type=text, tool_use, ...).
Default format writes formatted prose/tool logs. This script accepts either form,
concatenates assistant text, then extracts the review JSON object.
"""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path


def parse_ndjson_text(raw: str) -> str:
    """Collect assistant text chunks from OpenCode --format json output."""
    chunks: list[str] = []
    for line in raw.splitlines():
        line = line.strip()
        if not line.startswith("{"):
            continue
        try:
            event = json.loads(line)
        except json.JSONDecodeError:
            continue

        event_type = event.get("type")
        part = event.get("part") or {}

        if event_type == "text" and part.get("text"):
            chunks.append(part["text"])
            continue

        # Some OpenCode builds nest assistant content differently.
        if event_type == "message" and isinstance(event.get("content"), str):
            chunks.append(event["content"])
            continue

        messages = event.get("messages")
        if isinstance(messages, list):
            for message in messages:
                if not isinstance(message, dict):
                    continue
                content = message.get("content")
                if isinstance(content, str):
                    chunks.append(content)
                elif isinstance(content, list):
                    for block in content:
                        if isinstance(block, dict) and block.get("type") == "text":
                            text = block.get("text")
                            if text:
                                chunks.append(text)

    return "".join(chunks)


def extract_json_object(text: str) -> dict:
    text = text.strip()
    if not text:
        raise ValueError("empty text")

    try:
        parsed = json.loads(text)
        if isinstance(parsed, dict):
            return parsed
    except json.JSONDecodeError:
        pass

    fenced = re.search(r"```(?:json)?\s*(\{.*?\})\s*```", text, re.DOTALL)
    if fenced:
        return json.loads(fenced.group(1))

    start = text.find("{")
    end = text.rfind("}")
    if start >= 0 and end > start:
        return json.loads(text[start : end + 1])

    raise ValueError("Could not find JSON object in OpenCode output")


def extract(raw: str, stderr: str = "") -> dict:
    raw = raw.strip()
    if not raw and stderr.strip():
        raw = stderr.strip()

    if not raw:
        raise ValueError("OpenCode produced no output")

    # 1) Plain JSON file (already normalized)
    try:
        parsed = json.loads(raw)
        if isinstance(parsed, dict):
            return parsed
    except json.JSONDecodeError:
        pass

    # 2) NDJSON event stream from `opencode run --format json`
    ndjson_text = parse_ndjson_text(raw)
    if ndjson_text.strip():
        try:
            return extract_json_object(ndjson_text)
        except (json.JSONDecodeError, ValueError):
            pass

    # 3) Default formatted CLI output (prose + optional JSON body)
    return extract_json_object(raw)


def fallback_review(raw: str, stderr: str, reason: str) -> dict:
    combined = "\n".join(part for part in (raw.strip(), stderr.strip()) if part)
    snippet = combined[:4000] if combined else "(empty)"
    if len(combined) > 4000:
        snippet += "\n...(truncated)"

    return {
        "summary": (
            "OpenCode did not produce parseable review JSON.\n\n"
            f"**Reason:** {reason}\n\n"
            "Check the `opencode-raw.txt` workflow artifact for full CLI output.\n\n"
            f"**Output preview:**\n```\n{snippet}\n```"
        ),
        "comments": [],
    }


def main() -> int:
    src = Path(sys.argv[1] if len(sys.argv) > 1 else "opencode-raw.txt")
    dst = Path(sys.argv[2] if len(sys.argv) > 2 else "review.json")
    stderr_path = Path(sys.argv[3]) if len(sys.argv) > 3 else Path("opencode-stderr.txt")

    raw = src.read_text(encoding="utf-8", errors="replace") if src.exists() else ""
    stderr = (
        stderr_path.read_text(encoding="utf-8", errors="replace")
        if stderr_path.exists()
        else ""
    )

    try:
        data = extract(raw, stderr)
    except (ValueError, json.JSONDecodeError) as exc:
        print(f"WARNING: {exc}; writing fallback review.json", file=sys.stderr)
        data = fallback_review(raw, stderr, str(exc))

    if not isinstance(data, dict):
        data = fallback_review(raw, stderr, "OpenCode output was not a JSON object")

    if "summary" not in data:
        data["summary"] = "AI code review"
    if "comments" not in data or not isinstance(data["comments"], list):
        data["comments"] = []

    dst.write_text(json.dumps(data, indent=2), encoding="utf-8")
    print(f"Wrote {dst} with {len(data['comments'])} comment(s)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
