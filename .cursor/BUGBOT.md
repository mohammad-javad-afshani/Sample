# Bugbot — Sample API Review Rules

> **Important:** Bugbot reads this file from the **default branch** (e.g. `master`). Merge config changes before expecting them on feature PRs.

## Project summary

.NET 7 e-commerce + vendor analytics API. CQRS via MediatR, EF Core persistence, API-key auth on admin routes.

**Shared standards:** [docs/REVIEW_PROJECT_CONTEXT.md](../docs/REVIEW_PROJECT_CONTEXT.md)

## Modular rules (Bugbot follows these links)

- [Security & validation](bugbot/rules/dotnet-security.md)
- [Performance & data access](bugbot/rules/dotnet-performance.md)
- [Architecture & maintainability](bugbot/rules/dotnet-architecture.md)

## High-priority checks (always comment if violated)

| Area | Look for |
|------|----------|
| Security | `FromSqlRaw` + string interpolation; public export/list without auth; PII in logs |
| Correctness | `Add()` / `Update()` without `SaveChangesAsync`; `return null` instead of exception |
| Domain | Direct field mutation bypassing domain methods (`StockQuantity`, etc.) |
| Async | `Task.Run` + `.GetResult()`; fire-and-forget HTTP |
| Resilience | Webhook/notification before DB commit; infinite/unbounded retry |
| Cache | `IMemoryCache.Set` without expiry; global cache keys without tenant/id |
| Tests | `Assert.True(true)` or tests with no behavior assertion |

## Pairwise review (precision)

When two handlers solve the same concern differently in one PR (e.g. one validates, one does not), **flag the weaker implementation**. Do not request changes to code that already matches the good pattern in the same PR.

## Severity guidance

- **Block merge:** SQL injection, missing auth on sensitive data, missing persistence, payment/refund data loss
- **Strong comment:** N+1, unbounded export, swallowed exceptions, misleading public API names
- **Avoid noise:** formatting, naming nitpicks on unchanged legacy code

## Module map

- Checkout: `CreateOrderDraft` → `ReserveStock` → `ProcessPayment`
- Vendors: `Application/Vendors/`, `VendorController`
- Refunds: `Application/Refunds/`, `RefundController`

Trace cross-file workflows; do not review files in isolation.
