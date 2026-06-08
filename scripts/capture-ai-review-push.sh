#!/usr/bin/env bash
# Records pre-push baseline into docs/AI_REVIEW_RUN_MANIFEST.json for Run B metrics.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MANIFEST="$ROOT/docs/AI_REVIEW_RUN_MANIFEST.json"
COMMIT="$(git -C "$ROOT" rev-parse HEAD)"
UTC="$(date -u +"%Y-%m-%dT%H:%M:%SZ")"
COUNT="$(find "$ROOT/Domain/Refunds" "$ROOT/Application/Refunds" "$ROOT/Application/Inventory" \
  "$ROOT/Infrastructure/Repositories/RefundRepository.cs" \
  "$ROOT/Infrastructure/ExternalServices/RefundGatewayClient.cs" \
  "$ROOT/WebApplication1/Controllers/RefundController.cs" \
  -name '*.cs' 2>/dev/null | wc -l | tr -d ' ')"
STAT="$(git -C "$ROOT" diff --stat HEAD 2>/dev/null | tail -1 || true)"

python3 <<PY
import json
from pathlib import Path
p = Path("$MANIFEST")
data = json.loads(p.read_text())
data["prePush"] = {
    "commit": "$COMMIT",
    "capturedAtUtc": "$UTC",
    "csFileCountRound3": int("$COUNT"),
    "diffStat": """$STAT""".strip() or None,
}
p.write_text(json.dumps(data, indent=2) + "\n")
print("Updated prePush in", p)
print("  commit:", "$COMMIT")
print("  round3 .cs files:", "$COUNT")
PY

echo ""
echo "Next: commit Round 3, push, then fill docs/AI_REVIEW_RUN_METRICS.md after reviews."
