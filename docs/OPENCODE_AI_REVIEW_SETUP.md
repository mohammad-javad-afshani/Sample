# OpenCode AI Review — Setup Guide (GitHub Actions)

Automated **inline** PR review (comments on specific lines in **Files changed**) using [OpenCode](https://opencode.ai) CLI + GitHub Reviews API.

> OpenCode’s official GitHub Action still posts a **single summary comment** ([issue #13918](https://github.com/anomalyco/opencode/issues/13918)). This repo uses a **custom pipeline** for CodeRabbit-style inline threads.

---

## Files

| File | Purpose |
|------|---------|
| [`AI_REVIEW.md`](../AI_REVIEW.md) | Review rules |
| [`opencode.json`](../opencode.json) | Default agent: `plan` (read-only) |
| [`.github/workflows/ai-review.yml`](../.github/workflows/ai-review.yml) | **Auto inline review** on every PR |
| [`.github/prompts/inline-review-prompt.md`](../.github/prompts/inline-review-prompt.md) | Prompt → JSON output |
| [`.github/scripts/build-review-context.sh`](../.github/scripts/build-review-context.sh) | Numbered diff context for accurate lines |
| [`.github/scripts/normalize-review-json.py`](../.github/scripts/normalize-review-json.py) | Parse OpenCode output → `review.json` |
| [`.github/scripts/post-inline-review.py`](../.github/scripts/post-inline-review.py) | Post GitHub **Pull Request Review** with inline comments |
| [`.github/workflows/opencode-on-demand.yml`](../.github/workflows/opencode-on-demand.yml) | Optional `/opencode` chat (summary style) |

---

## Architecture (context-aware inline review)

```
PR opened / updated
        ↓
build-review-context.sh
  • diff.patch (what changed)
  • numbered changed files (inline line numbers)
  • valid-lines.json (GitHub-attachable lines)
        ↓
generate-review-openai.py  ← CI default (reliable)
  OR opencode run            ← optional (REVIEW_ENGINE=opencode, local)
        ↓
review.json
        ↓
post-inline-review.py → inline comments on changed lines only
```

**Why not OpenCode CLI in CI?** `opencode run` in headless Actions is fragile (agent selection, `-f`/`--` parsing, NDJSON extraction). CI uses the **same prompt and context** via OpenAI API directly. Use OpenCode locally for interactive review:

```bash
bash .github/scripts/build-review-context.sh <base-sha> <head-sha>
REVIEW_ENGINE=opencode bash .github/scripts/run-inline-review.sh review.json
```

---

## Step 1 — Add API secret

**Settings → Secrets and variables → Actions → New repository secret**

| Secret | Provider |
|--------|----------|
| `ANTHROPIC_API_KEY` | Claude (recommended) |
| `OPENAI_API_KEY` | OpenAI |

Configure provider/model in [`opencode.json`](../opencode.json) or OpenCode global config if needed.

---

## Step 2 — Push workflow to default branch

```bash
git add AI_REVIEW.md opencode.json .github/
git commit -m "Add OpenCode inline PR review workflow"
git push origin master
```

---

## Step 3 — What the workflow does (step-by-step)

### 3.1 Checkout

Full git history (`fetch-depth: 0`) at the PR head commit.

### 3.2 Build review context

Script: `.github/scripts/build-review-context.sh`

- Lists changed files vs base branch (`master`)
- Writes `.review-context/diff.patch`
- For each changed file, writes a **numbered** snapshot (line numbers = **new file version**, used as `line` in JSON)

### 3.3 Run OpenCode

- CLI: `opencode run --format json --agent plan --dangerously-skip-permissions`
- Reads [`.github/prompts/inline-review-prompt.md`](../.github/prompts/inline-review-prompt.md)
- OpenCode emits **NDJSON events** on stdout; `normalize-review-json.py` collects `type=text` chunks and extracts the review JSON
- Model output schema:

```json
{
  "summary": "Overall risk and summary",
  "comments": [
    {
      "path": "Application/Vendors/Update/UpdateVendorCommandHandler.cs",
      "line": 25,
      "side": "RIGHT",
      "severity": "high",
      "body": "Explanation + optional ```suggestion block"
    }
  ]
}
```

### 3.4 Post inline review

Script: `.github/scripts/post-inline-review.py`

- Calls `POST /repos/{owner}/{repo}/pulls/{pull_number}/reviews`
- `body` = summary markdown
- `comments[]` = inline threads on diff lines
- Invalid paths/lines are listed in the summary footer (not lost)
- Lines are validated against each file's PR diff hunks; out-of-diff lines snap to the nearest valid line (within 20 lines) or move to the summary footer

---

## Step 4 — Where comments appear

| Location | What you see |
|----------|----------------|
| **Files changed** | Inline threads next to code (like CodeRabbit) |
| **Conversation** | Review summary from the same submission |
| **Actions → Artifacts** | `review.json` + raw OpenCode log if something fails |

Supports GitHub **` ```suggestion `** blocks for one-click apply when the model includes them.

---

## Step 5 — Test

1. Open a PR → `master`
2. **Actions** → **AI Code Review (OpenCode Inline)**
3. Open **Files changed** — look for inline comments

Re-run: push a new commit to the PR.

---

## Step 6 — On-demand (optional)

[`opencode-on-demand.yml`](../.github/workflows/opencode-on-demand.yml) uses the official OpenCode action for **conversational** `/opencode` commands (not inline). Use auto workflow for structured reviews.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| No inline comments, only summary | Check `review.json` artifact — empty `comments` or wrong `line` numbers |
| Comments in summary “could not attach inline” | Path not in PR or line not on RIGHT side of diff — fix prompt or line numbers |
| `File not found: Follow inline...` | Prompt was parsed as a `-f` attachment — use `--` before the message in `opencode run` |
| OpenCode fails / no JSON | Verify `OPENAI_API_KEY` or `ANTHROPIC_API_KEY`; read `opencode-raw.txt` + `opencode-stderr.txt` artifacts. Workflow posts a fallback summary if JSON cannot be parsed. |
| `Could not find JSON object` | CI now uses `generate-review-openai.py` by default instead of OpenCode CLI |
| Test OpenCode locally | `REVIEW_ENGINE=opencode bash .github/scripts/run-inline-review.sh review.json` |
| Workflow exits with code **128** | Git could not diff base vs head — fixed by using `pull_request.base.sha` / `head.sha` instead of `origin/master` (base tip may not be fetched when checkout uses head SHA only) |
| Node.js 20 deprecation warning | Harmless for now; workflow sets `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24` and uses Node 24 for OpenCode install |
| Too many comments | Prompt caps at 15; script caps at 50 per review |
| Wrong model | Set provider in `~/.config/opencode/` or project `opencode.json` |

---

## Limitations

- Line numbers depend on model accuracy; numbered `.review-context/` files reduce errors but don’t eliminate them.
- Not a replacement for human review on large/security-critical merges.
- Rate limits: very large PRs may need path filters (future enhancement).

---

## Security

- API keys only in GitHub Secrets
- `plan` agent — read-only analysis
- `GITHUB_TOKEN` scoped to repository for the workflow run

---

## Official links

- [OpenCode docs](https://opencode.ai/docs)
- [GitHub: Create a review](https://docs.github.com/en/rest/pulls/reviews#create-a-review-for-a-pull-request)
- [OpenCode inline comments feature request](https://github.com/anomalyco/opencode/issues/13918)
