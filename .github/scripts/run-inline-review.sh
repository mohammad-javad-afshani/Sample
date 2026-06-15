#!/usr/bin/env bash
# Single entry point for CI inline review generation.
# Default: OpenAI API (reliable). Set REVIEW_ENGINE=opencode to try CLI first.
set -euo pipefail

REVIEW_ENGINE="${REVIEW_ENGINE:-openai}"
OUT="${1:-review.json}"
PR_TITLE="${2:-PR inline review}"

generate_openai() {
  python3 .github/scripts/generate-review-openai.py "$OUT"
}

try_opencode() {
  echo "Trying OpenCode CLI (REVIEW_ENGINE=opencode)..." >&2
  set +e
  cat .github/prompts/inline-review-prompt.md | opencode run \
    --format json \
    --agent build \
    --model openai/gpt-4o \
    --dangerously-skip-permissions \
    --title "$PR_TITLE" \
    > opencode-raw.txt 2> opencode-stderr.txt
  local exit_code=$?
  set -e
  python3 .github/scripts/normalize-review-json.py opencode-raw.txt "$OUT" opencode-stderr.txt

  if python3 - <<'PY'
import json, sys
from pathlib import Path
p = Path(sys.argv[1])
d = json.loads(p.read_text())
comments = d.get("comments") or []
summary = d.get("summary") or ""
if comments:
    sys.exit(0)
if "did not produce parseable" in summary.lower():
    sys.exit(1)
if "no high-confidence" in summary.lower():
    sys.exit(0)
sys.exit(1)
PY
  "$OUT"; then
    echo "OpenCode produced usable review.json" >&2
    return 0
  fi
  echo "OpenCode output unusable (exit $exit_code), falling back to OpenAI API..." >&2
  return 1
}

case "$REVIEW_ENGINE" in
  opencode)
    if try_opencode; then
      exit 0
    fi
    generate_openai
    ;;
  openai|*)
    generate_openai
    ;;
esac
