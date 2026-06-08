# PR: Vendor Analytics Module (Round 4 — Three-Way Benchmark)

## Summary

Adds **Vendor Analytics API** for supplier onboarding and reporting:

- Vendor CRUD (create with validation, update, list, search, export)
- Vendor metrics and report generation
- External analytics insight client

**Benchmark Run ID:** `R4-THREE-WAY-20250608`  
**Compare:** Greptile · Cursor · CodeRabbit on the **same commit**.

## Test plan

- [ ] Run migration `AddVendorsModule`
- [ ] `POST /Vendor/Create` with `X-Api-Key` — valid body
- [ ] `GET /Vendor/List?page=1&pageSize=20`
- [ ] `GET /Vendor/Search?term=acme`
- [ ] `GET /Vendor/Export` — check auth expectations
- [ ] `POST /Vendor/{id}/Report?date=2025-06-08`

## Notes for reviewers

Production-critical vendor data path. Please review:

- Input validation consistency across endpoints
- SQL and search implementation
- Logging of sensitive fields
- Auth on read vs write endpoints
- Pagination and query efficiency
- Cache usage in report generation
- External service resilience (retries, timeouts)
- Test quality

Scoring guide: [`docs/AI_REVIEW_ROUND4_BENCHMARK.md`](docs/AI_REVIEW_ROUND4_BENCHMARK.md)
