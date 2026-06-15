# AI Code Review — Sample Repository

Primary instructions for **OpenCode** (GitHub Actions) and any CLI-based PR reviewer.

**Extended context:** [docs/REVIEW_PROJECT_CONTEXT.md](docs/REVIEW_PROJECT_CONTEXT.md)  
**Checklist:** [AGENTS.md](AGENTS.md)

---

## Project architecture

| Layer | Path | Rules |
|-------|------|-------|
| Domain | `Domain/` | No EF, HTTP, or infrastructure references |
| Application | `Application/` | MediatR handlers, FluentValidation; no direct `DbContext` except read queries where established |
| Infrastructure | `Infrastructure/` | EF Core, repositories, HTTP clients, migrations |
| API | `WebApplication1/` | Controllers, `ApiKeyAuth`, configuration |

**Patterns:** .NET 7 · ASP.NET Core · CQRS (MediatR) · DDD-style aggregates · SQL Server · EF Core

**Modules:** Commerce/Orders, Payments, Promotions, Refunds, Vendors

---

## Review priorities (in order)

1. **Security** — SQL injection (`FromSqlRaw` + interpolation), missing auth on sensitive endpoints, secrets/PII in logs or API responses
2. **Correctness** — missing `IUnitOfWork.SaveChangesAsync` after Add/Update, `return null` instead of typed exceptions, swallowed exceptions
3. **Domain integrity** — direct field mutation bypassing domain methods (e.g. `StockQuantity` vs `ReserveStock()`)
4. **Performance** — N+1 queries, unbounded `ToListAsync`, sync-over-async (`Task.Run`, `.Result`), cache without TTL or wrong keys
5. **Resilience** — webhooks/HTTP before DB commit, unbounded retry loops, empty catch on external calls
6. **Architecture** — SRP violations, misleading names, boolean flag parameters, dead/duplicate code
7. **Tests** — `Assert.True(true)` placeholders; missing tests for new validation rules

---

## Authentication

Sensitive operations use **`X-Api-Key`** via `WebApplication1/ApiKeyAuth.cs` (constant-time compare).

**Flag:** bulk export/list/metrics endpoints without auth while Create/Update/Report require auth.

---

## Pairwise precision

When two handlers solve the same concern differently in one PR (e.g. one validates, one does not):

- **Flag the weaker implementation**
- **Do not** request changes to code that already matches the good pattern in the same PR

---

## Reporting rules

- **Only high-confidence findings** — issues you can point to with file + line + concrete fix
- **Ignore** formatting, style-only nits, legacy nullable warnings on unchanged entities
- **Ignore** migration designer filenames unless the migration logic is wrong
- **Output:** Markdown with Summary + table (Severity | File | Issue | Fix)
- **Cap:** ~15 findings; prioritize blockers (security, data loss, auth)
- **Severity labels:** Blocker · High · Medium · Low

---

## Block merge if found

- SQL injection or raw interpolated SQL with user input
- Unauthenticated access to bulk financial/PII data
- PII (tax ID, internal notes, payment amounts) in logs
- Missing persistence after successful mutation response
