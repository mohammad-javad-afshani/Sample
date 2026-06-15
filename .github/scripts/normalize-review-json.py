#!/usr/bin/env python3
"""Extract review.json from OpenCode CLI output.

OpenCode `run --format json` writes NDJSON events (type=text, tool_use, ...).
The final assistant text should contain {"summary": ..., "comments": [...]}.
"""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path


def is_ndjson_stream(raw: str) -> bool:
    typed_lines = 0
    for line in raw.splitlines():
        line = line.strip()
        if not line.startswith("{"):
            continue
        try:
            event = json.loads(line)
        except json.JSONDecodeError:
            continue
        if isinstance(event, dict) and "type" in event:
            typed_lines += 1
            if typed_lines >= 2:
                return True
    return False


def parse_ndjson_text_events(raw: str) -> list[str]:
    """Return each assistant text chunk from OpenCode --format json output."""
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
    return chunks


def looks_like_review(obj: dict) -> bool:
    return isinstance(obj, dict) and "summary" in obj and "comments" in obj


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

    # Prefer objects that look like our review schema.
    for match in re.finditer(r"\{", text):
        start = match.start()
        end = text.rfind("}", start)
        while end > start:
            candidate = text[start : end + 1]
            try:
                parsed = json.loads(candidate)
            except json.JSONDecodeError:
                end = text.rfind("}", start, end)
                continue
            if isinstance(parsed, dict):
                return parsed
            break

    raise ValueError("Could not find JSON object in OpenCode output")


def find_review_json(texts: list[str]) -> dict:
    # Final assistant messages usually contain the deliverable.
    for text in reversed(texts):
        try:
            candidate = extract_json_object(text)
        except (json.JSONDecodeError, ValueError):
            continue
        if looks_like_review(candidate):
            return candidate

    for text in reversed(texts):
        try:
            candidate = extract_json_object(text)
        except (json.JSONDecodeError, ValueError):
            continue
        if isinstance(candidate, dict):
            return candidate

    combined = "\n".join(texts)
    candidate = extract_json_object(combined)
    if isinstance(candidate, dict):
        return candidate
    raise ValueError("Could not find review JSON in OpenCode text events")


def extract(raw: str, stderr: str = "") -> dict:
    raw = raw.strip()
    if not raw and stderr.strip():
        raw = stderr.strip()

    if not raw:
        raise ValueError("OpenCode produced no output")

    # Plain review.json already
    if not is_ndjson_stream(raw):
        try:
            parsed = json.loads(raw)
            if isinstance(parsed, dict) and looks_like_review(parsed):
                return parsed
        except json.JSONDecodeError:
            pass

    # NDJSON stream from `opencode run --format json`
    if is_ndjson_stream(raw):
        texts = parse_ndjson_text_events(raw)
        if texts:
            return find_review_json(texts)
        raise ValueError("OpenCode NDJSON stream contained no text events")

    # Default formatted CLI output (prose + optional JSON body)
    return extract_json_object(raw)


def fallback_review(raw: str, stderr: str, reason: str) -> dict:
    combined = "\n".join(part for part in (raw.strip(), stderr.strip()) if part)
    snippet = combined[:4000] if combined else "(empty)"
    if len(combined) > 4000:
        snippet += "\n...(truncated)"

    hint = ""
    if "plan" in snippet.lower() or "### plan" in snippet.lower():
        hint = (
            "\n\n**Hint:** OpenCode used the `plan` agent and emitted a plan instead of review JSON. "
            "The workflow should use `--agent review`."
        )

    return {
        "summary": (
            "OpenCode did not produce parseable review JSON.\n\n"
            f"**Reason:** {reason}{hint}\n\n"
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
