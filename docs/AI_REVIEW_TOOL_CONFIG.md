# AI Review Tool Configuration — Greptile · Cursor Bugbot · CodeRabbit

How to get **best review performance** on this repo for the three-way benchmark (Run ID: `R4-THREE-WAY-20250608`).

---

## Quick reference — config files in repo

| Tool | Config location | Merged to default branch? |
|------|-----------------|---------------------------|
| **Cursor Bugbot** | [`.cursor/BUGBOT.md`](../.cursor/BUGBOT.md) + [`.cursor/bugbot/rules/`](../.cursor/bugbot/rules/) | **Yes — required** |
| **Greptile** | [`.greptile/`](../.greptile/) folder | Yes (read from PR source branch) |
| **CodeRabbit** | [`.coderabbit.yaml`](../.coderabbit.yaml) + [`AGENTS.md`](../AGENTS.md) | Yes (read from PR branch) |
| **Shared context** | [`docs/REVIEW_PROJECT_CONTEXT.md`](REVIEW_PROJECT_CONTEXT.md) | Referenced by all |

---

## 1. Cursor Bugbot

**Official docs:** [cursor.com/docs/bugbot](https://cursor.com/docs/bugbot)

### What Bugbot reads

| Source | Used by Bugbot? |
|--------|-----------------|
| `.cursor/BUGBOT.md` (root + nested per folder) | **Yes** |
| `.cursor/bugbot/rules/*.md` (via markdown links) | **Yes** |
| `.cursor/rules/` (IDE agent rules) | **No** — separate system |
| `BUGBOT.md` at repo root (without `.cursor/`) | **No** — use `.cursor/BUGBOT.md` |

### Setup checklist

1. Enable **Bugbot** on the GitHub repo (Cursor Dashboard → Bugbot).
2. **Merge** `.cursor/BUGBOT.md` to `master` first — Bugbot uses default-branch config, not unmerged PR files.
3. Optional: enable **Learned rules** in dashboard; teach inline with `@cursor remember …` on PRs.
4. Optional: add **Team rules** in dashboard for org-wide standards.

### Files we added

```
.cursor/BUGBOT.md                          # Project-wide rules + links
.cursor/bugbot/rules/dotnet-security.md
.cursor/bugbot/rules/dotnet-performance.md
.cursor/bugbot/rules/dotnet-architecture.md
Application/Vendors/.cursor/BUGBOT.md      # Nested rules for vendor module
```

### Re-run

Push a new commit or re-trigger from Cursor dashboard on the PR.

---

## 2. Greptile

**Official docs:** [greptile.com/docs/code-review/greptile-config](https://www.greptile.com/docs/code-review/greptile-config)

### What Greptile reads (priority order)

1. Org enforced rules (dashboard)
2. **`.greptile/` folder** (recommended — we use this)
3. `greptile.json` (ignored if `.greptile/` exists)
4. Dashboard defaults

### Setup checklist

1. Connect repo at [app.greptile.com](https://app.greptile.com).
2. Enable review on PR open + updates (`triggerOnUpdates: true` in our config).
3. **Strict mode:** `strictness: 1` in `.greptile/config.json` (= verbose, flags more issues).
4. Dashboard: set **full repo indexing** (not diff-only) if available.

### Files we added

```
.greptile/config.json    # strictness, rules with id/scope/severity
.greptile/rules.md       # Full checklist (your 4 sections)
.greptile/files.json     # Points to REVIEW_PROJECT_CONTEXT.md, AGENTS.md, etc.
```

### Re-run

Push to PR or use **Re-trigger Greptile** link in PR comment.

---

## 3. CodeRabbit

**Official docs:** [docs.coderabbit.ai](https://docs.coderabbit.ai/getting-started/yaml-configuration)

### What CodeRabbit reads

| Source | Auto-loaded? |
|--------|--------------|
| `.coderabbit.yaml` on **PR branch** | Yes |
| `AGENTS.md` | Yes (code guidelines) |
| Path-specific `path_instructions` | Yes |
| `.cursor/BUGBOT.md` | No |

### Setup checklist

1. Install CodeRabbit GitHub app on the repo.
2. Profile: **`assertive`** (already in `.coderabbit.yaml`).
3. `base_branches` includes benchmark branches (`test-code-review-3`, etc.).
4. After a few PRs: comment `@coderabbitai emit path instructions` to refine rules.

### Files we added

- [`.coderabbit.yaml`](../.coderabbit.yaml) — path-specific instructions per layer
- [`AGENTS.md`](../AGENTS.md) — checklist CodeRabbit picks up automatically

### Re-run

Comment `@coderabbitai review` on the PR.

---

## 4. Fair comparison checklist

- [ ] All config files **merged to master** before benchmark PR (especially Bugbot)
- [ ] Same **commit** reviewed by all three tools
- [ ] Greptile strictness = **1** (repo file, not dashboard-only)
- [ ] CodeRabbit profile = **assertive**
- [ ] Bugbot enabled on repository
- [ ] Do **not** paste answer key into PR description
- [ ] Score with [`AI_REVIEW_ROUND4_SCORING_TEMPLATE.md`](AI_REVIEW_ROUND4_SCORING_TEMPLATE.md)

---

## 5. Troubleshooting

| Problem | Fix |
|---------|-----|
| Bugbot ignores rules | Merge `.cursor/BUGBOT.md` to default branch first |
| Greptile too quiet | Confirm `.greptile/config.json` on PR branch; `strictness: 1` |
| Greptile too noisy | Set `strictness: 2` or `3` |
| CodeRabbit misses security | Check `path_instructions` for `Infrastructure/` and `Controllers/` |
| Duplicate rules | Bugbot ≠ `.cursor/rules/` — maintain shared doc in `docs/REVIEW_PROJECT_CONTEXT.md` |

---

## 6. Dashboard-only settings (cannot be in repo)

| Tool | Set in dashboard |
|------|------------------|
| Greptile | Org defaults, repo indexing mode, enforced rules |
| CodeRabbit | Org overrides, central config inheritance |
| Cursor Bugbot | Team rules, learned rules, enable/disable per repo |

Repository files **override** dashboard defaults for Greptile and CodeRabbit where documented.
