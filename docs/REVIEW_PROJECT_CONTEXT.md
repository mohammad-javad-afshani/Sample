# Sample API — Review Project Context

Use this document as the **single source of truth** for AI code review (Greptile, CodeRabbit, Cursor Bugbot, OpenCode).

## What this repository is

Production-style **.NET 7** sample for e-commerce + vendor analytics:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain | `Domain/` | Entities, value objects, domain exceptions, repository interfaces |
| Application | `Application/` | MediatR commands/queries, FluentValidation, handlers |
| Infrastructure | `Infrastructure/` (`Persistence`) | EF Core, repositories, HTTP clients, migrations |
| API | `WebApplication1/` | ASP.NET Core controllers, auth, configuration |

**Patterns:** CQRS (MediatR), repository + unit of work, DDD-style aggregates (`Product`, `Order`, `Vendor`, …).

## Critical review priorities (in order)

1. **Security** — SQL injection, auth on endpoints, secrets in config/logs, sensitive fields in API responses
2. **Correctness** — missing `SaveChangesAsync`, wrong persistence, null returns, swallowed exceptions
3. **Domain integrity** — bypass domain methods (e.g. direct `StockQuantity` mutation vs `ReserveStock()`)
4. **Performance** — N+1 queries, unbounded lists, sync-over-async, missing pagination caps
5. **Resilience** — webhook/HTTP ordering vs DB commit, retry loops, fire-and-forget, missing timeouts
6. **Maintainability** — SRP, misleading names, boolean flag parameters, dead code, duplicate logic
7. **Tests** — meaningful assertions (not placeholder `Assert.True(true)`)

## Authentication

Admin/mutating endpoints use **`X-Api-Key`** checked via `WebApplication1/ApiKeyAuth.cs` (constant-time compare).

**Flag:** endpoints that mutate data or expose PII without calling `IsAuthorized()` / `ApiKeyAuth`.

## Persistence rules

- Handlers that call `repository.Add()` or `Update()` must commit via **`IUnitOfWork.SaveChangesAsync`** unless part of an explicit outer transaction.
- Prefer **typed domain exceptions** over `return null` or bare `throw new Exception("failed")`.

## Data access rules

- **Never** interpolate user input into `FromSqlRaw` / dynamic SQL — use EF LINQ or parameterized queries.
- Avoid **`SELECT *`** when exposing internal columns; use DTOs/projections.
- Cap **`page` / `pageSize`** on list endpoints (see `ListVendorsQueryHandler` pattern).

## External services

HTTP clients are registered in `Infrastructure/DependencyInjection.cs` with `BaseUrl` + `TimeoutSeconds` from `appsettings.json`.

**Flag:** unbounded retry loops, empty catch blocks on gateway calls, dispatch before DB commit.

## Logging

- Use structured logging (`ILogger`).
- **Do not log** tax IDs, payment/refund amounts, internal notes, or API keys.
- Match log level to outcome (`LogInformation` for success, `LogError` for failures).

## Modules (for context mapping)

| Module | Key paths |
|--------|-----------|
| Commerce | `Application/Orders/`, `Application/Payments/`, `CommerceController` |
| Promotions | `Application/Promotions/`, `PromotionController` |
| Refunds | `Application/Refunds/`, `RefundController` |
| Vendors | `Application/Vendors/`, `VendorController`, `Domain/Vendors/` |

When reviewing a PR, connect handlers in the **same workflow** (checkout, refund, vendor report) — bugs often span multiple files.

## What NOT to nitpick

- Nullable reference warnings on legacy `Customer`/`Address` entities unless changed in the PR
- Migration designer file naming (`initdbcontex`, etc.)
- Style-only changes unrelated to correctness/security

## Precision expectation

Compare **similar code in the same PR**: if one handler validates input and another does not, flag the inconsistent (weaker) path. Do not suggest breaking working **control** patterns that already follow the rules above.
