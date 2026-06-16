# Bugbot Rule: Security (.NET)

**Scope:** `Application/**`, `Infrastructure/**`, `WebApplication1/**`

## Input validation

- All new **commands** must have FluentValidation (or equivalent) before persistence.
- Compare with existing validators in the same feature folder — flag handlers that skip validation when siblings validate.

## Injection

- Flag any `FromSqlRaw`, `ExecuteSqlRaw`, or string-built SQL with user input.
- Flag missing parameterization in repository search methods.

## Authentication & authorization

- Mutating endpoints and endpoints returning PII/financial data must use `ApiKeyAuth.IsAuthorized` or equivalent.
- Flag `GET` list/export endpoints that return all records without auth when sibling `POST` routes require auth.

## Secrets & sensitive data

- Flag hardcoded secrets (HMAC keys, API keys) in source.
- Flag `LogInformation`/`LogDebug` that includes: tax ID, refund/payment amounts, internal notes, full API keys.
- Flag API responses exposing `InternalNotes` or internal cost fields without auth.

## Error handling

- Do not return `null` for missing entities — use typed exceptions (`*NotFoundException`).
- Do not use empty `catch { return; }` on financial/gateway paths.
