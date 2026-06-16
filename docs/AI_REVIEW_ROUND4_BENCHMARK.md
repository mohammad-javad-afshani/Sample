# Round 4 Benchmark — Greptile vs Cursor vs CodeRabbit

**Run ID:** `R4-THREE-WAY-20250608`  
**Branch:** `test-code-review-3`  
**Module:** Vendor Analytics API (paired **GOOD** / **BAD** code)  
**Tools under test:** Greptile · **Cursor** (Bugbot / Agent Review) · CodeRabbit

---

## Design principle

For each measurable review criterion we embed:

| Pair | Purpose | Reviewer should |
|------|---------|-----------------|
| **GOOD (control)** | Correct pattern in production-style code | **Not** flag as defect (may praise) |
| **BAD (target)** | Same criterion violated nearby | **Comment** with actionable finding |

No inline markers like `// BUG` — reviewers must infer from context.

---

## How to run

1. Push branch `test-code-review-3` and open **PR #3** (or reuse PR #2 on this branch).
2. Enable all three reviewers on the **same commit**.
3. Export PR comments (Greptile summary + inline, CodeRabbit, Cursor).
4. Score using [`AI_REVIEW_ROUND4_SCORING_TEMPLATE.md`](AI_REVIEW_ROUND4_SCORING_TEMPLATE.md).
5. Compare tools — send PR link + exports for automated fill-in.

**Answer key (local only):** `docs/internal/AI_REVIEW_ROUND4_ANSWER_KEY.md` (gitignored)

---

## Scenario catalog (30 measurable targets)

Each row maps to your checklist + metric categories **A–G**.

| ID | Checklist theme | BAD (must flag) | GOOD (control) | Cat |
|----|-----------------|-----------------|----------------|-----|
| **R4-01** | Input validation | `UpdateVendorCommandHandler` — no validator | `CreateVendorCommandHandler` + `CreateVendorValidator` | A, C |
| **R4-02** | SQL injection | `VendorRepository.SearchUnsafeAsync` + `SearchVendorsQueryHandler` | `VendorRepository.ListPagedAsync` + `ListVendorsQueryHandler` | C |
| **R4-03** | Sensitive data in logs | `ExportVendorsQueryHandler` logs TaxId + InternalNotes | `CreateVendorCommandHandler` logs only VendorId | C, Resilience |
| **R4-04** | Access control | `GET /Vendor/Export` — no auth | `POST /Vendor/Create` — `ApiKeyAuth` | C |
| **R4-05** | Over-exposure in API | `ExportVendorsQuery` `IncludeInternalFields=true` default | `ListVendorsQuery` projects public fields only | C |
| **R4-06** | Return null | `GetVendorByCodeQueryHandler` returns null | `UpdateVendor` throws `VendorNotFoundException` | Error handling |
| **R4-07** | Exception without context | `AnalyticsInsightClient` `throw new Exception("failed")` | Typed domain exceptions elsewhere | Error handling |
| **R4-08** | Empty catch / swallow | `AnalyticsInsightClient` bare `catch` retry loop | `ProcessRefund` (fixed) rethrows after MarkFailed | Resilience |
| **R4-09** | Boolean flag parameters | `ExportVendorsQuery(bool SkipValidation, bool IncludeInternalFields)` | Commands with explicit intent | Maintainability |
| **R4-10** | Disinforming names | `GetUserOrderStatsQuery` returns **vendor** metrics | `ListVendorsQuery` / `VendorMetricItem` naming | Maintainability |
| **R4-11** | N+1 queries | `GetUserOrderStatsQueryHandler` loop + Count per vendor | `ListVendorsQueryHandler` single paged query | D |
| **R4-12** | Pagination / unbounded data | `ExportVendorsQueryHandler` loads **all** rows | `ListVendorsQueryHandler` `MaxPageSize=100` | D |
| **R4-13** | SELECT * / over-fetch | `SearchUnsafeAsync` `SELECT *` | `ListPagedAsync` tracked projection | D |
| **R4-14** | Sync-over-async / blocking | `SearchVendorsQueryHandler` `Task.Run` + `.GetResult()` | `ListVendorsQueryHandler` async await | D, B |
| **R4-15** | Cache without TTL | `GenerateVendorReportCommandHandler` `_memoryCache.Set("report", …)` no expiry | (Reviewer should ask for TTL/sliding) | D, Caching |
| **R4-16** | Cache key collision | Same handler uses key `"report"` for all vendors | Should include vendorId + date in key | D, Caching |
| **R4-17** | Shared mutable static state | `static Dictionary` `SharedReportCache` | Instance-scoped repo/cache | B, D |
| **R4-18** | Retry without limit / overload | `AnalyticsInsightClient` `while(true)` up to 100 | HttpClient timeout in DI (10s) | Resilience |
| **R4-19** | No circuit breaker | Tight retry on external analytics call | Document as partial if only timeout noted | Resilience |
| **R4-20** | SRP / god handler | `GenerateVendorReportCommandHandler` file + email + DB + cache | `CreateVendorCommandHandler` single purpose | E, F |
| **R4-21** | Duplicate switch / DRY | `BuildSummary` + `FormatSummaryForEmail` identical switches | — | F |
| **R4-22** | Dead code | `UnusedLegacyFormat` private method never called | — | F |
| **R4-23** | Wrong log level | `LogError` for successful report generation | `LogInformation` on create | Observability |
| **R4-24** | Shotgun surgery risk | Report handler touches unrelated `UpdateContact` | — | F |
| **R4-25** | Test quality (bad) | `VendorPlaceholderTests.Placeholder_always_passes` | — | G |
| **R4-26** | Test quality (good) | — | `VendorCreateValidationTests` asserts email validation | G |
| **R4-27** | Auth on read vs write | `Search`, `List`, `Metrics`, `Lookup` anonymous | `Create`, `Update`, `Report` protected | C |
| **R4-28** | Dependency / secrets in repo | (Prior rounds) appsettings keys — optional cross-ref | Webhooks signing from config (fixed) | C |
| **R4-29** | Algorithmic waste | O(vendors × products) nested loop in metrics | O(page) list endpoint | D |
| **R4-30** | Actionability check | Any finding on **GOOD** control code only | True positive only on **BAD** rows | Precision |

**Target count:** **26 BAD** scenarios reviewers should comment · **8 GOOD** controls they should not mis-flag.

---

## Checklist coverage map (your full list → Round 4)

### 1. Critical Security & Core Stability

| Your item | Round 4 scenario |
|-----------|------------------|
| SQL injection | R4-02 |
| Sanitizers / raw input | R4-01, R4-02 |
| Sensitive data in logs | R4-03 |
| Sensitive data in API response | R4-05 |
| Access control (auth) | R4-04, R4-27 |
| Input validation | R4-01 (pair) |
| Meaningful naming | R4-10 |
| Boolean flag params | R4-09 |
| Dead / commented code | R4-22 |
| Exceptions vs return codes | R4-06, R4-07 |
| Don’t return null | R4-06 |
| Log failures | R4-08, R4-23 |

### 2. Performance & Efficiency

| Your item | Round 4 scenario |
|-----------|------------------|
| Pagination | R4-12 (pair) |
| N+1 | R4-11 |
| Avoid SELECT * | R4-13 |
| Async I/O | R4-14 |
| Cache TTL / keys | R4-15, R4-16 |
| Unbounded payload | R4-12 |
| Connection timeout | R4-18 (DI config) |
| Retry limit | R4-18 |
| O-notation waste | R4-29 |

### 3. Resilience & Observability

| Your item | Round 4 scenario |
|-----------|------------------|
| Circuit breaker | R4-19 (partial credit) |
| Retry / fallback | R4-18 |
| Log levels | R4-23 |
| Sensitive masking | R4-03 |
| Tests FIRST | R4-25, R4-26 |

### 4. Maintainability & Architecture

| Your item | Round 4 scenario |
|-----------|------------------|
| SRP | R4-20 |
| DRY / duplicate switch | R4-21 |
| Cohesion / modularity | Vendor folder structure |
| Design smell (long params) | R4-09 |

---

## Weighted score model (your metrics)

| Metric | Weight | How to measure on Round 4 |
|--------|--------|---------------------------|
| **True Positive Rate** | 25% | Found BAD / 26 |
| **Severity Detection** | 20% | P0/P1 or Critical/Major on security/reliability BAD |
| **Architectural Findings** | 15% | R4-20, R4-21, R4-24 flagged |
| **Security Findings** | 10% | R4-02, R4-03, R4-04, R4-05 flagged |
| **Context Awareness** | 10% | Links vendor module to commerce; no false “fix CreateVendor validation” |
| **Precision** | 10% | 1 − (FP on GOOD controls / total comments) |
| **Actionability** | 5% | Suggested fix or clear remediation text |
| **Noise Reduction** | 5% | Inverse of comment count at same severity |

**Category tags (A–G):**

| Tag | Scenarios |
|-----|-----------|
| A Functional | R4-01, R4-06, R4-12 |
| B Concurrency | R4-14, R4-17 |
| C Security | R4-02–R4-05, R4-27 |
| D Performance | R4-11–R4-16, R4-29 |
| E Architecture | R4-20 |
| F Maintainability | R4-09, R4-10, R4-21, R4-22 |
| G Test quality | R4-25, R4-26 |

---

## Files introduced (Round 4)

```
Domain/Vendors/
Application/Vendors/Create/     ← GOOD validation
Application/Vendors/Update/     ← BAD no validation
Application/Vendors/Search/     ← BAD SQL + blocking
Application/Vendors/List/       ← GOOD pagination
Application/Vendors/Export/     ← BAD logs + flags + bulk
Application/Vendors/Metrics/    ← BAD N+1 + bad naming
Application/Vendors/Lookup/     ← BAD null return
Application/Vendors/Reports/    ← BAD cache/SRP/static/log level
Application/Analytics/
Infrastructure/Repositories/VendorRepository.cs
Infrastructure/ExternalServices/AnalyticsInsightClient.cs
WebApplication1/Controllers/VendorController.cs
Test/Vendors/VendorBenchmarkTests.cs
```

---

## After review — what to send back

- PR URL  
- Exports: Greptile, CodeRabbit, **Cursor** review output  
- Optional: timestamps for speed comparison  

We fill `AI_REVIEW_ROUND4_SCORING_TEMPLATE.md` and produce a three-way report.
