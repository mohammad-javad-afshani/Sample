#!/usr/bin/env python3
"""Build path -> valid RIGHT-side line numbers from a unified diff patch."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path


def parse_unified_diff(patch_text: str) -> dict[str, set[int]]:
    files: dict[str, set[int]] = {}
    current_path: str | None = None
    new_line = 0
    in_hunk = False

    for line in patch_text.splitlines():
        if line.startswith("diff --git "):
            current_path = None
            in_hunk = False
            continue
        if line.startswith("+++"):
            match = re.match(r"^\+\+\+ b/(.+)$", line)
            if match:
                current_path = match.group(1)
                files.setdefault(current_path, set())
            continue
        if current_path is None:
            continue
        if line.startswith("@@"):
            match = re.search(r"\+(\d+)(?:,(\d+))?", line)
            if match:
                new_line = int(match.group(1))
                in_hunk = True
            continue
        if not in_hunk:
            continue
        if line.startswith("+"):
            files[current_path].add(new_line)
            new_line += 1
        elif line.startswith("-"):
            continue
        elif line.startswith(" "):
            files[current_path].add(new_line)
            new_line += 1

    return files


def main() -> int:
    patch_path = Path(sys.argv[1] if len(sys.argv) > 1 else ".review-context/diff.patch")
    out_path = Path(sys.argv[2] if len(sys.argv) > 2 else ".review-context/valid-lines.json")

    if not patch_path.exists():
        out_path.write_text("{}\n", encoding="utf-8")
        print(f"No patch at {patch_path}; wrote empty {out_path}")
        return 0

    parsed = parse_unified_diff(patch_path.read_text(encoding="utf-8"))
    serializable = {path: sorted(lines) for path, lines in sorted(parsed.items())}
    out_path.write_text(json.dumps(serializable, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {out_path} ({len(serializable)} file(s))")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
