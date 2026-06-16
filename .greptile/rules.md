# Greptile Review Rules — Sample API

See also structured rules in `config.json`. Full project context: [docs/REVIEW_PROJECT_CONTEXT.md](../docs/REVIEW_PROJECT_CONTEXT.md)

## 1. Critical security & stability

- SQL injection (parameterized queries only)
- Input validation on all commands
- Auth on sensitive reads/writes (`X-Api-Key` / `ApiKeyAuth`)
- No secrets or PII in logs or public responses
- Typed exceptions; no bare `Exception("failed")` or silent catch

## 2. Performance & efficiency

- No N+1 in loops
- Pagination caps on list endpoints
- Async await for I/O; no blocking
- Cache: TTL + unique keys
- No unbounded export of full tables

## 3. Resilience & observability

- External HTTP: bounded retry, no empty catch
- Webhooks/notifications after DB commit
- Appropriate log levels; mask sensitive fields
- Meaningful tests (FIRST principles)

## 4. Maintainability & architecture

- SRP: one reason to change per handler
- DDD: use domain methods, not field hacks
- Clear naming (no disinforming method names)
- Avoid boolean flag parameters
- Remove dead code; deduplicate switch blocks

## Pairwise precision

If the PR contains both a **correct** pattern and a **weaker** pattern for the same concern, comment on the weaker one only. Do not treat the correct pattern as a defect.

## Workflow context

Connect related files in checkout, refund, and vendor report flows. Include Important Files table and Security Review section in summary.
