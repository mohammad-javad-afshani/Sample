#!/usr/bin/env bash
# Builds review context: PR diff + numbered changed files + fixed architecture context bundle.
set -euo pipefail

BASE_REF="${1:?base ref required}"
HEAD_SHA="${2:?head sha required}"
OUT_DIR=".review-context"

rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR/context"

git fetch origin "$BASE_REF" --depth=1 2>/dev/null || true

mapfile -t FILES < <(git diff --name-only "origin/${BASE_REF}...${HEAD_SHA}" | grep -v '^$' || true)

# Always available for cross-file / pairwise review (not necessarily in the diff)
CONTEXT_FILES=(
  "AI_REVIEW.md"
  "AGENTS.md"
  "docs/REVIEW_PROJECT_CONTEXT.md"
  "WebApplication1/ApiKeyAuth.cs"
  ".cursor/BUGBOT.md"
)

{
  echo "# Changed files in this PR (inline comments must target these paths)"
  for f in "${FILES[@]}"; do
    echo "- $f"
  done
  echo ""
  echo "# Context files (read for patterns — do NOT flag unless referenced by changed code)"
  for f in "${CONTEXT_FILES[@]}"; do
    echo "- $f"
  done
} > "$OUT_DIR/INDEX.md"

# Numbered snapshots of changed files (for accurate inline line numbers)
for f in "${FILES[@]}"; do
  safe="${f//\//__}"
  if git cat-file -e "${HEAD_SHA}:${f}" 2>/dev/null; then
    git show "${HEAD_SHA}:${f}" | nl -ba > "$OUT_DIR/${safe}.txt"
  fi
done

# Copy context files from PR head (full content for sibling/pattern comparison)
for f in "${CONTEXT_FILES[@]}"; do
  if git cat-file -e "${HEAD_SHA}:${f}" 2>/dev/null; then
    dest="$OUT_DIR/context/${f//\//__}"
    mkdir -p "$(dirname "$dest")"
    git show "${HEAD_SHA}:${f}" > "$dest"
  fi
done

# Related files in same module folders as changed paths (e.g. Create vs Update handlers)
declare -A SEEN=()
for f in "${FILES[@]}"; do
  dir="$(dirname "$f")"
  parent="$(dirname "$dir")"
  for scan in "$dir" "$parent"; do
    [ "$scan" = "." ] && continue
    while IFS= read -r related; do
      [ -z "$related" ] && continue
      [[ -n "${SEEN[$related]:-}" ]] && continue
      SEEN[$related]=1
      if git cat-file -e "${HEAD_SHA}:${related}" 2>/dev/null; then
        safe=".review-context/related/${related//\//__}.txt"
        mkdir -p "$(dirname "$safe")"
        git show "${HEAD_SHA}:${related}" | nl -ba > "$safe"
        echo "- $related (related module — context only)" >> "$OUT_DIR/INDEX.md"
      fi
    done < <(git ls-tree -r --name-only "$HEAD_SHA" "$scan" 2>/dev/null | head -40)
  done
done

git diff "origin/${BASE_REF}...${HEAD_SHA}" > "$OUT_DIR/diff.patch"
python3 .github/scripts/build-valid-lines-from-patch.py "$OUT_DIR/diff.patch" "$OUT_DIR/valid-lines.json"
echo "Context built: ${#FILES[@]} changed file(s), ${#CONTEXT_FILES[@]} fixed context file(s)"
