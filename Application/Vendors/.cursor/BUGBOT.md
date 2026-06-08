# Bugbot — Vendor module

Extra context when reviewing `Application/Vendors/**`, `Domain/Vendors/**`, or `VendorController`.

## Scope

Vendor CRUD, search, export, metrics, external analytics integration, and report generation.

## Focus areas

- **Validation consistency** across Create vs Update command handlers
- **Search** implementation in repository (raw SQL vs LINQ)
- **Auth** consistency across controller actions (read vs write)
- **Export** volume, logging, and response shape
- **Metrics** query efficiency (avoid per-row DB calls)
- **Report** handler: caching, retries, logging levels, single responsibility
- **External client** `AnalyticsInsightClient`: retry policy and error handling

## Reference

See root [.cursor/BUGBOT.md](../../.cursor/BUGBOT.md) and [docs/REVIEW_PROJECT_CONTEXT.md](../../docs/REVIEW_PROJECT_CONTEXT.md).
