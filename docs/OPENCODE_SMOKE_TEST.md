# OpenCode smoke test PR

This branch exists to validate the **AI Code Review (OpenCode Inline)** GitHub Actions workflow.

## What changed

| Change | Purpose |
|--------|---------|
| `AGENTS.md` | Review checklist referenced by `AI_REVIEW.md` and context builder |
| `docs/REVIEW_PROJECT_CONTEXT.md` | Architecture and security rules for context-aware review |
| `Application/Health/*` + `HealthController` | Small feature with intentional review targets (see below) |

## Expected review findings

OpenCode should flag at least:

1. **Missing auth** on `GET /Health/Status` — other sensitive endpoints use `ApiKeyAuth`.
2. **Connection string in API response** — `HealthStatusResponse.DatabaseConnectionString` exposes secrets.
3. **Connection string in logs** — `HealthController` logs the full connection string.

After opening the PR to `master`, check **Actions → AI Code Review (OpenCode Inline)** and inline comments on the Files changed tab.
