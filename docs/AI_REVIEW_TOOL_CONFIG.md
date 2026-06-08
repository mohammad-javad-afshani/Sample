# Tool configuration — Greptile & CodeRabbit

Recommendations for the Sample PR #1 benchmark. Adjust in each tool's dashboard if repo files are not supported.

---

## CodeRabbit

A [`.coderabbit.yaml`](../.coderabbit.yaml) is included in the repo with:

| Setting | Value | Why |
|---------|-------|-----|
| `profile` | `assertive` | More findings on benchmark PR |
| `high_level_summary` | `true` | PR-level overview for scoring |
| `path_filters` | exclude `.md`, `.idea` | Focus on code |
| `path_instructions` | security + distributed focus | Steers toward R1-8, R2-9–R2-15 |

**Optional (dashboard):**

- Enable **security** and **performance** review lenses if available
- Set **base branch** to `master`
- Disable poem / low-value comments

After changing config, comment `@coderabbitai review` on the PR to re-run.

---

## Greptile

Greptile is configured via [greptile.com](https://greptile.com) dashboard (no repo file in this project).

**Recommended settings:**

| Setting | Recommendation |
|---------|----------------|
| Repository indexing | Full repo (not diff-only) |
| Review trigger | On every push to PR |
| Strictness | High / include architecture |
| Custom instructions | Same focus as CodeRabbit `path_instructions` |

**Suggested custom prompt (paste in Greptile config):**

```
Review for: missing SaveChangesAsync, domain method bypass, SQL injection,
hardcoded secrets, non-constant-time string compare, N+1 queries, pagination
count bugs, idempotency on payments, webhook fired before DB commit,
fire-and-forget HTTP, cancellation token ignored, coupon race conditions.
```

Re-trigger by pushing a new commit or using Greptile's "re-review" on the PR.

---

## Fair comparison checklist

- [ ] Both tools run on the **same commit**
- [ ] Same PR scope (don't filter files differently)
- [ ] Record **inline comments** + **summary review** from each
- [ ] Score using [`AI_REVIEW_SCORING_TEMPLATE.md`](AI_REVIEW_SCORING_TEMPLATE.md)
- [ ] Do **not** paste answer key into PR description

---

## When to change config

| If this happens | Action |
|-----------------|--------|
| Too many style nits | Narrow path_instructions; reduce assertive profile |
| Missing security findings | Enable security lens; add SQL/secrets to Greptile prompt |
| Missing idempotency | Add distributed-systems keywords to both prompts |
| False positives > 3 | Subtract in noise column; tune profile to `chill` for daily use |
