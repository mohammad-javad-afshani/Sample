# Bugbot Rule: Architecture & maintainability (.NET)

**Scope:** `Application/**`, `Domain/**`, `Test/**`

## Domain-driven design

- Application handlers must use domain methods (`ReserveStock`, `AdjustStock`, etc.) not direct property mutation for invariants.
- Domain layer must not reference EF Core, HTTP, or ASP.NET.

## Single responsibility

- Flag handlers that combine unrelated concerns: file I/O + email + DB + cache in one `Handle` method.

## Naming & API clarity

- Flag methods/queries whose names disagree with return data (e.g. "User" in name, "Vendor" in data).
- Flag boolean flag parameters controlling behavior (`skipValidation`, `force`, `includeInternal` defaults).

## Code quality

- Flag duplicate switch/if blocks that should be shared.
- Flag clearly dead private methods never called.
- Flag wrong log levels (`LogError` on success paths).

## Tests

- Flag tests that always pass without asserting behavior (`Assert.True(true)`).
- Require meaningful tests for new validation rules.

## Distributed systems

- Flag external notifications/webhooks dispatched **before** `SaveChangesAsync`.
- Flag missing idempotency on payment/refund gateway calls when concurrent requests are possible.
