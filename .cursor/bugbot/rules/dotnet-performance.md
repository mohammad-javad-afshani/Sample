# Bugbot Rule: Performance & efficiency (.NET)

**Scope:** `Application/**`, `Infrastructure/Repositories/**`

## Database

- Flag **N+1**: loops that query DB per iteration (reviews, products, vendors).
- Flag **`SELECT *`** or loading full entities when DTO projection suffices.
- Flag list/export handlers that load **all rows** without pagination.
- Flag `totalCount` computed on unfiltered query when filters apply.

## Async I/O

- Flag `.GetAwaiter().GetResult()`, `.Result`, `Task.Run` wrapping async repository calls.
- Flag fire-and-forget HTTP (`Task.Run` without await, `Task.CompletedTask` after starting work).

## Caching

- Flag `IMemoryCache.Set` without expiration options.
- Flag static/shared mutable caches (`static Dictionary`) used across requests.
- Flag cache keys that omit entity id, tenant, or date (collision risk).

## External services

- Flag retry loops without max attempts or exponential backoff.
- Flag missing use of configured `HttpClient` timeout (tight spin retry instead).

## API payload

- Flag endpoints returning unbounded collections where paginated alternatives exist in the same module.
