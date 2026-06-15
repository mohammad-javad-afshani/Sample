#!/usr/bin/env python3
"""Extract review.json from OpenCode stdout (plain JSON or markdown fenced)."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path


def extract(raw: str) -> dict:
    raw = raw.strip()
    if not raw:
        return {"summary": "OpenCode produced no output.", "comments": []}
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        pass
    m = re.search(r"```(?:json)?\s*(\{.*\})\s*```", raw, re.DOTALL)
    if m:
        return json.loads(m.group(1))
    start, end = raw.find("{"), raw.rfind("}")
    if start >= 0 and end > start:
        return json.loads(raw[start : end + 1])
    raise ValueError("Could not find JSON object in OpenCode output")


def main() -> int:
    src = Path(sys.argv[1] if len(sys.argv) > 1 else "opencode-raw.txt")
    dst = Path(sys.argv[2] if len(sys.argv) > 2 else "review.json")
    raw = src.read_text(encoding="utf-8") if src.exists() else ""
    data = extract(raw)
    if "summary" not in data:
        data["summary"] = "AI code review"
    if "comments" not in data:
        data["comments"] = []
    dst.write_text(json.dumps(data, indent=2), encoding="utf-8")
    print(f"Wrote {dst} with {len(data['comments'])} comment(s)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
