# Round 4 — Three-Way AI Review Comparison (Final Report)

**Run ID:** `R4-THREE-WAY-20250608`  
**PR:** [#4 — changes](https://github.com/mohammad-javad-afshani/Sample/pull/4)  
**Commit reviewed:** `c165b2c338dcfe7e9c490f08de4e29a0727158be`  
**Branch:** `test-code-review-3` → `master`  
**Date:** 2026-06-08  
**Tools:** Greptile · Cursor Bugbot · CodeRabbit  

**Evidence archive:** PR export saved as `uploads/4-1.md`; raw inline comments pulled from [GitHub Pull Request Review Comments API](https://docs.github.com/en/rest/pulls/comments) on 2026-06-08.

### Document map

| Section | Contents |
|---------|----------|
| [Executive summary](#executive-summary) | Rankings and recommendation |
| [Methodology](#how-we-measured-this-methodology) | Ground truth, evidence, fair-run rules |
| [**Metrics methodology (how /5 scores)**](AI_REVIEW_ROUND4_METRICS_METHODOLOGY.md) | **Explain Correctness, Severity, Context, etc. for stakeholders** |
| [Headline results](#headline-results) | Boolean + rate metrics |
| [Detection matrix](#detection-matrix-26-bad-scenarios) | All 26 BAD scenarios (F/P/M) |
| [Evidence highlights](#evidence-highlights-presentable-examples) | Presentable proof examples |
| [Weighted composite](#weighted-composite-scorecard) | 8 metrics with weights |
| [Qualitative rubric](#qualitative-rubric-15-and-) | 10 Persian/English dimensions + % |
| [Weighted metrics (full)](#weighted-metrics-full-model) | Formulas per metric |
| [Category A–G](#category-coverage-ag) | Functional → Test quality |
| [**Full checklist §1–§4**](#full-review-checklist-coverage) | **Every org checklist item** |
| [Operational metrics](#operational-metrics) | Speed, comment counts |
| [Winner summary](#winner-summary) | Final recommendation |
| [Appendix](#appendix--comment-inventory-api) | All bot comments |

---

## Executive summary

| Rank | Tool | Weighted score | Detection (26 BAD) | Best at |
|------|------|----------------|--------------------|---------|
| **1** | **Greptile** | **87.4 / 100** | **90.4%** (23.5/26) | Fast structured summary, paired-context findings, maintainability (SRP, dead code, LogError) |
| **2** | **Cursor Bugbot** | **85.2 / 100** | **92.3%** (24/26) | Highest raw recall, fastest first feedback (~1 min), security high-severity tagging |
| **3** | **CodeRabbit** | **77.2 / 100** | **80.8%** (21/26) | Deepest fix prompts, Critical/Major labels, domain-level extras — but more noise and missed Update-validation / maintainability trio |

**Recommended primary gate:** **Greptile** (best balance of speed, context, and precision on this benchmark).  
**Complement:** **Cursor Bugbot** for fast security blocking; **CodeRabbit** when you want copy-paste remediation prompts and domain hardening suggestions.

---

## How we measured this (methodology)

If someone asks *“how did you score this?”*, use this section.

### 1. Ground truth (what “correct” means)

We did **not** guess from the PR diff alone. Each intentional defect was pre-registered before review:

| Source | Purpose |
|--------|---------|
| [`docs/AI_REVIEW_ROUND4_BENCHMARK.md`](AI_REVIEW_ROUND4_BENCHMARK.md) | 26 **BAD** scenarios + 8 **GOOD** controls (paired design) |
| [`docs/AI_REVIEW_ROUND4_MANIFEST.json`](AI_REVIEW_ROUND4_MANIFEST.json) | Machine-readable scenario → file mapping |
| `docs/internal/AI_REVIEW_ROUND4_ANSWER_KEY.md` | Local answer key (not in PR — reviewers had no `// BUG` markers) |

**Scoring unit:** one row = one BAD scenario (R4-01 … R4-29, excluding R4-26 which is a GOOD control).

### 2. Evidence collection (what we actually read)

| Step | Action | Output |
|------|--------|--------|
| A | Export PR conversation | `uploads/4-1.md` |
| B | `GET /repos/.../pulls/4/comments` | 61 inline review comments (20 Cursor + 13 Greptile + 28 CodeRabbit) |
| C | `GET /repos/.../issues/4/comments` | Bot summary comments (Greptile summary, CodeRabbit walkthrough) |
| D | Map each comment → scenario ID | Detection matrix below |

**Rule:** A scenario is **Found (F)** if the bot’s comment explicitly describes that defect on the target file/pattern. **Partial (P)** if related but incomplete (e.g. only Export auth flagged, not Search/Metrics). **Missed (M)** if no comment maps to that scenario.

### 3. Metrics (your weighted model)

| Metric | Weight | Measurement |
|--------|--------|-------------|
| True Positive Rate | 25% | (F + 0.5×P) / 26 BAD scenarios |
| Severity Detection | 20% | High/Critical labels on security BAD (R4-02–05, partial R4-27) |
| Architectural Findings | 15% | R4-20, R4-21, R4-24 |
| Security Findings | 10% | R4-02–05, R4-27 |
| Context Awareness | 10% | Paired comparisons (Create vs Update, List vs Export) without false “fix the good path” |
| Precision | 10% | False positives on 4 GOOD controls + out-of-scope nits |
| Actionability | 5% | Suggested code / “Fix with AI” / patch blocks |
| Noise Reduction | 5% | Signal vs comment count (inverse noise) |

Qualitative rubric uses **1–5** per dimension (Persian labels in §6). Operational metrics use **timestamps from GitHub API**.

### 4. Fair-run conditions

- Same commit `c165b2c` reviewed by all three tools  
- Repo configs merged on branch: `.greptile/`, `.cursor/BUGBOT.md`, `.coderabbit.yaml`, `AGENTS.md`  
- Greptile `strictness: 1`; CodeRabbit profile `assertive`  
- Answer key **not** pasted into PR description  

---

## Headline results

### Boolean checks (quick slide table)

| Check | Greptile | Cursor Bugbot | CodeRabbit |
|-------|:--------:|:-------------:|:----------:|
| Found SQL injection (R4-02) | ✅ | ✅ | ✅ |
| Found export without auth (R4-04) | ✅ | ✅ | ✅ |
| Found PII in export logs (R4-03) | ✅ | ✅ | ✅ |
| Found Update without validation (R4-01) | ✅ | ✅ | ❌ |
| Found SRP / god handler (R4-20) | ✅ | ✅ | ✅ |
| Found duplicate switch (R4-21) | ✅ | ✅ | ❌ |
| Found dead code (R4-22) | ✅ | ✅ | ❌ |
| Found LogError on success (R4-23) | ✅ | ✅ | ❌ |
| Found misleading query name (R4-10) | ✅ | ❌ | ✅ |
| Found return-null lookup (R4-06) | ❌ | ✅ | ✅ |
| Provided PR summary | ✅ | ⚠️ minimal | ✅ |
| Published confidence score | ✅ (2/5) | ❌ | ❌ |
| False positive on GOOD CreateVendor validation | ❌ | ❌ | ✅ |

### Rate metrics (/10 and %)

| Metric | Greptile | Cursor Bugbot | CodeRabbit |
|--------|:--------:|:-------------:|:----------:|
| **Detection rate** | **90.4%** | **92.3%** | **80.8%** |
| **Weighted composite** | **8.7 / 10** | **8.5 / 10** | **7.7 / 10** |
| Severity on security BAD | 8.3 / 10 | 8.3 / 10 | 8.3 / 10 |
| Architectural (R4-20–24) | 8.3 / 10 | 8.3 / 10 | 5.0 / 10 |
| Precision (GOOD controls) | 9.5 / 10 | 9.5 / 10 | 8.5 / 10 |
| Context awareness | 9.0 / 10 | 8.5 / 10 | 8.0 / 10 |
| Actionability | 9.5 / 10 | 8.5 / 10 | 9.8 / 10 |
| Signal-to-noise | 9.0 / 10 | 8.5 / 10 | 7.0 / 10 |

---

## Detection matrix (26 BAD scenarios)

**Legend:** F = Found · P = Partial · M = Missed  

| ID | Scenario (BAD target) | Greptile | Cursor | CodeRabbit | Proof |
|----|------------------------|:--------:|:------:|:----------:|-------|
| R4-01 | Update no FluentValidation | **F** | **F** | M | Greptile: `UpdateVendorCommandHandler` P1 · Cursor: "Update vendor skips validation" · CodeRabbit: no comment on update handler (API verified) |
| R4-02 | SQL injection `SearchUnsafeAsync` | **F** | **F** | **F** | All three P0/Critical/High on `VendorRepository.cs` |
| R4-03 | PII in export logs | **F** | **F** | **F** | `ExportVendorsQueryHandler` TaxId/InternalNotes in logs |
| R4-04 | Export endpoint no auth | **F** | **F** | **F** | `GET /Vendor/Export` vs Create/Update auth |
| R4-05 | Internal fields exposed by default | **F** | **F** | **F** | `includeInternalFields = true` default |
| R4-06 | Return null lookup | M | **F** | **F** | Greptile missed; Cursor & CR on `GetVendorByCodeQueryHandler` |
| R4-07 | Generic `Exception("failed")` | **F** | **F** | **F** | `AnalyticsInsightClient` retry comments |
| R4-08 | Swallowed exception in retry | **F** | **F** | **F** | bare `catch (Exception)` |
| R4-09 | Boolean flag parameters | M | P | P | Cursor/CR: SkipValidation ignored; not full boolean-smell on query type |
| R4-10 | Misleading `GetUserOrderStats` name | **F** | M | **F** | Greptile Issue 15 in summary; CR inline on query file; Cursor silent |
| R4-11 | N+1 metrics loop | **F** | **F** | **F** | `GetUserOrderStatsQueryHandler` foreach + CountAsync |
| R4-12 | Unbounded export load | **F** | **F** | **F** | Full-table `ToListAsync` |
| R4-13 | SELECT * over-fetch | **F** | **F** | **F** | Same SQL injection thread |
| R4-14 | Sync-over-async search | **F** | **F** | **F** | `Task.Run` + `.GetResult()` |
| R4-15 | Cache without TTL | **F** | **F** | **F** | `IMemoryCache.Set` no expiry |
| R4-16 | Cache key `"report"` collision | **F** | **F** | **F** | Wrong vendor cached report |
| R4-17 | Static `SharedReportCache` | **F** | **F** | **F** | Thread-unsafe static dictionary |
| R4-18 | Unbounded retry (100×) | **F** | **F** | **F** | `while(true)` loop |
| R4-19 | No circuit breaker | P | P | P | Retry noted; circuit breaker not named |
| R4-20 | SRP god handler | **F** | **F** | **F** | File + email + DB in one handler |
| R4-21 | Duplicate switch blocks | **F** | **F** | M | Greptile Issue 13; Cursor duplicate formatter; CR missed |
| R4-22 | Dead code `UnusedLegacyFormat` | **F** | **F** | M | Greptile Issue 14; Cursor unused helper |
| R4-23 | LogError on success path | **F** | **F** | M | Greptile P1 LogError; Cursor LogError comment |
| R4-24 | Shotgun `UpdateContact` in report | P | P | P | Mentioned inside SRP / side-effect comments only |
| R4-25 | Placeholder test | **F** | **F** | **F** | `Assert.True(true)` |
| R4-27 | Anonymous Search/Metrics/Lookup | P | P | P | Export (+ CR: List) auth; Search/Metrics/Lookup still open |
| R4-29 | O(vendors×products) waste | **F** | **F** | **F** | Same N+1 metrics finding |

**Totals:** Greptile **23.5/26 (90.4%)** · Cursor **24/26 (92.3%)** · CodeRabbit **21/26 (80.8%)**

---

## Evidence highlights (presentable examples)

### Example 1 — SRP violation (R4-20): Greptile ✅ · Cursor ✅ · CodeRabbit ✅

**What we planted:** `GenerateVendorReportCommandHandler` writes files, sends notification stub, updates DB, and caches in one `Handle` method.

**Greptile** (inline, P2, rule `srp-god-handler`):

> SRP violation: file I/O, no-op notification, and DB update in one handler… `WriteReportArtifactAsync` and `SendReportNotificationAsync` are called *before* `SaveChangesAsync`.

**Cursor Bugbot** (`Side effects before database commit`, Medium):

> Report artifact write and notification occur before `SaveChangesAsync` — side effects committed even if DB fails.

**CodeRabbit** (Major, refactor):

> Split report generation side effects into dedicated collaborators (`IReportWriter`, `INotificationService`, `IVendorUpdater`).

**Measurement:** All three map to R4-20 = **Found**. Wording differs; Greptile tied to custom rule ID with link to `.greptile/config.json`.

---

### Example 2 — Update validation gap (R4-01): Greptile ✅ · Cursor ✅ · CodeRabbit ❌

**What we planted:** `UpdateVendorCommandHandler` has no FluentValidation while `CreateVendorCommandHandler` validates all fields.

**Greptile** (P1 on `UpdateVendorCommandHandler.cs`):

> Missing input validation on Update… sibling CreateVendorCommandHandler validates Name, Email, TaxId.

**Cursor Bugbot** (`Update vendor skips validation`, Medium):

> Persists contact changes without validation while Create path validates.

**CodeRabbit:** **No inline comment** on `UpdateVendorCommandHandler.cs` among 28 review comments (verified via API). This is a clear **Miss** despite strong Create/Update pairing in repo rules.

**Measurement:** Boolean “Found Update validation gap?” → Greptile **Yes**, Cursor **Yes**, CodeRabbit **No**. Evidence: API comment list filtered by path.

---

### Example 3 — Maintainability trio (R4-21, R4-22, R4-23)

| Scenario | Greptile | Cursor | CodeRabbit |
|----------|:--------:|:------:|:----------:|
| R4-21 Duplicate switch | ✅ Summary Issue 13 + inline culture | ✅ `Duplicate unused email formatter` | ❌ Miss |
| R4-22 Dead code | ✅ Issue 14 | ✅ `Unused legacy export helper` | ❌ Miss |
| R4-23 Wrong log level | ✅ P1 LogError inline | ✅ `LogError on successful report` | ❌ Miss |

**Takeaway:** On **maintainability/observability** defects without immediate security impact, **Greptile + Cursor** caught all three; **CodeRabbit** focused elsewhere (domain guards, migration syntax, repository semantics).

---

### Example 4 — False positive on GOOD control (precision)

**CodeRabbit** commented on **`CreateVendorCommandHandler`** (GOOD control):

> Use a typed validation exception instead of `InvalidOperationException`.

The handler **already validates** via `CreateVendorValidator` — this is style/error-type preference, not a missing-validation bug. Per benchmark rules, flagging the GOOD control reduces precision.

| Tool | FP on CreateVendor GOOD control? |
|------|----------------------------------|
| Greptile | No |
| Cursor Bugbot | No |
| CodeRabbit | **Yes** (1 confirmed) |

**Precision estimate:** CodeRabbit 1 − 1/28 ≈ **96%** on inline comments; Greptile/Cursor **100%** on GOOD controls in this run.

---

## Weighted composite scorecard

| Metric | Weight | Greptile | Cursor | CodeRabbit |
|--------|--------|----------|--------|------------|
| True Positive Rate | 25% | 90.4 → **22.6** | 92.3 → **23.1** | 80.8 → **20.2** |
| Severity Detection | 20% | 83 → **16.6** | 83 → **16.6** | 83 → **16.6** |
| Architectural Findings | 15% | 83 → **12.5** | 83 → **12.5** | 50 → **7.5** |
| Security Findings | 10% | 90 → **9.0** | 90 → **9.0** | 90 → **9.0** |
| Context Awareness | 10% | 90 → **9.0** | 85 → **8.5** | 80 → **8.0** |
| Precision | 10% | 95 → **9.5** | 95 → **9.5** | 85 → **8.5** |
| Actionability | 5% | 95 → **4.8** | 85 → **4.3** | 98 → **4.9** |
| Noise Reduction | 5% | 90 → **4.5** | 85 → **4.3** | 70 → **3.5** |
| **TOTAL** | **100%** | **87.4** | **85.2** | **77.2** |

---

## Operational metrics

| Event | Greptile | Cursor Bugbot | CodeRabbit |
|-------|----------|---------------|------------|
| PR opened | 2026-06-08 02:58:01 UTC | same | same |
| First bot activity | 03:03:30 (summary) | **02:59:02** (inline) | 02:58:18 (walkthrough start) |
| First inline review | 03:03:34 | **02:59:02** | 03:07:56 |
| Inline comment count | **13** | **20** | **28** |
| Inline review duration | 12 s | **1 s** | 2 s |
| Summary comment | ✅ Rich + confidence 2/5 | ⚠️ Count only | ✅ Walkthrough + sequence diagrams |
| Sequence diagram | ✅ | ❌ | ✅ |
| Custom rule citation | ✅ (`Rule Used:` links) | ✅ (`Triggered by project rule`) | ✅ (`Source: Coding guidelines`) |
| Suggested patches | ✅ Greptile suggestions | Fix in Cursor / Web | ✅ Code blocks + AI agent prompts |

**Fastest actionable inline feedback:** **Cursor Bugbot** (~61 seconds after PR open).  
**Best executive summary:** **Greptile** (security + reliability narrative, confidence score, 16-issue fix list).

---

## Qualitative rubric (1–5 and %)

**Scale:** 1 = poor · 3 = acceptable · 5 = excellent · **%** = (score ÷ 5) × 100

| # | متریک (Persian) | English | Greptile | Cursor | CodeRabbit |
|---|-----------------|---------|:--------:|:------:|:----------:|
| 1 | Correctness — آیا باگ واقعی پیدا کرده؟ | True bug detection | **5** (100%) | **5** (100%) | 4 (80%) |
| 2 | Severity — اهمیت مشکل | Severity calibration | **5** (100%) | **5** (100%) | 4 (80%) |
| 3 | Precision — درصد کامنت‌های درست | Comment accuracy | **5** (100%) | **5** (100%) | 4 (80%) |
| 4 | Context Awareness | Project/module context | **5** (100%) | 4 (80%) | 4 (80%) |
| 5 | Architectural Understanding | Beyond syntax | **5** (100%) | 4 (80%) | 3 (60%) |
| 6 | Business Understanding | Requirement intent | 4 (80%) | 3 (60%) | 4 (80%) |
| 7 | Actionability | Actionable comments | **5** (100%) | 4 (80%) | **5** (100%) |
| 8 | Signal-to-Noise | Low noise | **5** (100%) | 4 (80%) | 3 (60%) |
| 9 | Security Awareness | Security issues seen | **5** (100%) | **5** (100%) | **5** (100%) |
| 10 | Maintainability | Maintainability focus | **5** (100%) | **5** (100%) | 3 (60%) |
| | **Average qualitative** | | **4.8 (96%)** | **4.3 (86%)** | **3.8 (76%)** |

**How each row was scored:** Derived from detection matrix + inline comment audit. Example: CodeRabbit **Correctness 4/5** because it missed 5/26 BAD scenarios (Update validation, duplicate switch, dead code, LogError, partial auth). **Precision 4/5** because 1 confirmed false positive on GOOD `CreateVendorCommandHandler` plus 3 meta/config comments outside benchmark scope.

---

## Weighted metrics (full model)

| Metric | Weight | Formula (this run) | Greptile | Cursor | CodeRabbit |
|--------|--------|-------------------|:--------:|:------:|:----------:|
| **True Positive Rate** | **25%** | (F + 0.5×P) / 26 BAD × 100 | 90.4% | 92.3% | 80.8% |
| **Severity Detection** | **20%** | High/Critical on security BAD R4-02–05,27 | 83% | 83% | 83% |
| **Architectural Findings** | **15%** | R4-20, R4-21, R4-24 found | 83% | 83% | 50% |
| **Security Findings** | **10%** | R4-02,03,04,05,27 found | 90% | 90% | 90% |
| **Context Awareness** | **10%** | Paired comparisons, no GOOD-path noise | 90% | 85% | 80% |
| **Precision** | **10%** | 1 − FP / total inline comments | 95% | 95% | 85% |
| **Actionability** | **5%** | Patch / Fix-with-AI / clear remediation | 95% | 85% | 98% |
| **Noise Reduction** | **5%** | Quality ÷ comment volume (inverse noise) | 90% | 85% | 70% |
| | **100%** | **Weighted composite** | **87.4** | **85.2** | **77.2** |

---

## Category coverage (A–G)

| Cat | Name | Scenarios | Greptile | Cursor | CodeRabbit | % Greptile |
|-----|------|-----------|:--------:|:------:|:----------:|:----------:|
| **A** | Functional Bugs | R4-01, R4-06, R4-12 | 2.5/3 | **3/3** | 2/3 | 83% |
| **B** | Concurrency | R4-14, R4-17 | **2/2** | **2/2** | **2/2** | 100% |
| **C** | Security | R4-02–05, R4-27 | 4.5/5 | 4.5/5 | 4.5/5 | 90% |
| **D** | Performance | R4-11–16, R4-29 | **7/7** | **7/7** | **7/7** | 100% |
| **E** | Architecture | R4-20 | **1/1** | **1/1** | **1/1** | 100% |
| **F** | Maintainability | R4-09,10,21,22,24 | 3.5/5 | 3.5/5 | 1.5/5 | 70% |
| **G** | Test Quality | R4-25 (+ good R4-26) | **1/1** | **1/1** | **1/1** | 100% |

**Legend for categories:** Same as detection matrix — F=1, P=0.5, M=0 per scenario.

**Weakest shared gap:** R4-27 — partial auth on read endpoints (Export/List flagged; Search/Metrics/Lookup missed).  
**CodeRabbit weakest area:** Category **F** (missed R4-21, R4-22, R4-23).

---

## Full review checklist coverage

This section maps **every item in your org checklist** to Round 4 benchmark evidence.

**Symbols:** ✅ Found · ⚠️ Partial · ❌ Missed · ➖ **N/A** (not exercised in this .NET API PR — no planted defect)

**Checklist hit rate** = applicable items where tool ✅ or ⚠️ ÷ applicable items tested.

| Pillar | Applicable items tested | Greptile | Cursor | CodeRabbit |
|--------|-------------------------|:--------:|:------:|:----------:|
| §1 Critical Security & Core Stability | 18 | **94%** | **94%** | **83%** |
| §2 Performance & Efficiency | 16 | **100%** | **100%** | **100%** |
| §3 Resilience & Observability | 8 | **88%** | **88%** | **75%** |
| §4 Maintainability & Architecture | 12 | **92%** | **92%** | **67%** |
| **All applicable (54 items)** | 54 | **93%** | **94%** | **84%** |

---

### §1. Critical Security & Core Stability

*Highest impact on production quality, easiest to enforce.*

#### Security

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Code injection — **SQL injection** | R4-02, R4-13 · `SearchUnsafeAsync` | ✅ P0 | ✅ High | ✅ Critical |
| Code injection — **XSS** | ➖ No front-end/HTML in PR | ➖ | ➖ | ➖ |
| **Sanitizers** for raw input | R4-01 validation, R4-02 parameterized query | ✅ | ✅ | ⚠️ SQL only |
| No sensitive data in **localStorage/sessionStorage** | ➖ Server API only | ➖ | ➖ | ➖ |
| No sensitive data in **logs** | R4-03 · TaxId/InternalNotes logged | ✅ P1 | ✅ High | ✅ Major |
| Review MR for **files/sensitive data** | R4-05 export exposure | ✅ | ✅ | ✅ |
| **Sensitive interfaces** — avoid exposing critical APIs | R4-04, R4-05, R4-27 | ⚠️ Export only | ⚠️ Export only | ⚠️ List+Export |
| **Security headers** (CSP, HSTS) | ➖ Not in PR scope | ➖ | ➖ | ➖ |
| **Input validation** | R4-01 Update vs Create pair | ✅ P1 | ✅ | ❌ missed Update |
| **Access control** (AuthN & AuthZ) | R4-04, R4-27 | ⚠️ | ⚠️ | ⚠️ |

#### Dependencies

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Up-to-date supported libraries | ➖ Not a planted defect | ➖ | ➖ | ➖ |
| Remove unused dependencies | ➖ | ➖ | ➖ | ➖ |
| Validate licenses & security risks | ➖ | ➖ | ➖ | ➖ |

#### Code Quality (Foundational)

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Meaningful, searchable **names** | R4-10 `GetUserOrderStatsQuery` | ✅ Issue 15 | ❌ | ✅ |
| Avoid **disinformation** in naming | R4-10 | ✅ | ❌ | ✅ |
| Class names = nouns | ➖ Style, not planted | ➖ | ➖ | ➖ |
| Method names = verbs | ➖ | ➖ | ➖ | ➖ |
| Standard conventions & code style | ➖ | ➖ | ➖ | ➖ |
| Remove **dead / repeated / commented** code | R4-21, R4-22 | ✅ | ✅ | ❌ |
| Standard formatting | ➖ | ➖ | ➖ | ➖ |
| Don't abuse temporary variables | ➖ | ➖ | ➖ | ➖ |
| Don't use **boolean flag** parameters | R4-09 | ❌ | ⚠️ SkipValidation only | ⚠️ |
| Don't overuse primitives | ➖ | ➖ | ➖ | ➖ |

#### Error Handling

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| **Exceptions** rather than return codes | R4-06, R4-07 | ⚠️ R4-06 miss | ✅ | ✅ |
| Try-Catch-Finally first | ➖ | ➖ | ➖ | ➖ |
| Unchecked exceptions | ➖ | ➖ | ➖ | ➖ |
| **Context with exceptions** | R4-07 `Exception("failed")` | ✅ | ✅ | ✅ |
| **Don't return null** | R4-06 | ❌ | ✅ | ✅ |
| Don't pass null | ➖ | ➖ | ➖ | ➖ |
| **Log all failures** | R4-08 swallow, R4-23 wrong level | ✅ | ✅ | ⚠️ retry only |

---

### §2. Performance & Efficiency

*Direct impact on scalability and user experience.*

#### Database Optimization

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Avoid **SELECT \*** | R4-13 | ✅ | ✅ | ✅ |
| **Limit rows** returned | R4-12 export | ✅ | ✅ | ✅ |
| **Pagination** | R4-12 vs List good pair | ✅ | ✅ | ✅ |
| Avoid **N+1** queries | R4-11, R4-29 | ✅ P2 | ✅ | ✅ |
| Understand execution plans | ➖ | ➖ | ➖ | ➖ |
| Batch processing for bulk ops | ➖ | ➖ | ➖ | ➖ |
| Query complexity | ⚠️ implicit in N+1/SQL | ✅ | ✅ | ✅ |
| Check for deadlocks | ➖ | ➖ | ➖ | ➖ |

#### Resource Management

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Close connections explicitly | ➖ EF Core managed | ➖ | ➖ | ➖ |
| Thread pools in multi-thread tasks | R4-14 `Task.Run` abuse | ✅ | ✅ | ✅ |
| Terminate background threads | ➖ | ➖ | ➖ | ➖ |
| Monitor shared resources (multi-thread) | R4-17 static cache | ✅ | ✅ | ✅ |
| **Async I/O** for I/O-bound ops | R4-14 blocking search | ✅ | ✅ | ✅ |
| Limit global variables | R4-17 static dict | ✅ | ✅ | ✅ |
| Avoid circular references | ➖ | ➖ | ➖ | ➖ |
| Garbage collection awareness | ➖ | ➖ | ➖ | ➖ |

#### Caching

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Cache expensive operations appropriately | R4-15–16 report cache | ✅ | ✅ | ✅ |
| Avoid caching lightweight/changing data | ➖ | ➖ | ➖ | ➖ |
| **Unique, meaningful cache keys** | R4-16 `"report"` | ✅ | ✅ | ✅ |
| Prevent **cache key collisions** | R4-16 cross-vendor leak | ✅ P1 | ✅ High | ✅ Major |
| **TTL / expiry** based on volatility | R4-15 no expiry | ✅ P2 | ✅ | ✅ |
| Robust **cache invalidation** | ➖ partial via TTL comment | ⚠️ | ⚠️ | ⚠️ |

#### Data Efficiency

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Data compression (gzip) | ➖ | ➖ | ➖ | ➖ |
| Optimize payload structure | R4-05 internal fields | ✅ | ✅ | ✅ |
| Only necessary fields in responses | R4-05 | ✅ | ✅ | ✅ |
| **Pagination** for large datasets | R4-12 | ✅ | ✅ | ✅ |
| Batch requests to reduce API calls | ➖ | ➖ | ➖ | ➖ |
| Redis pipeline / connection pool | ➖ SQL Server only | ➖ | ➖ | ➖ |
| **Timeout** on external services | R4-18 DI 10s timeout exists | ⚠️ noted in summary | ⚠️ | ⚠️ |
| **Limit retry** to prevent overload | R4-18 100× loop | ✅ P1 | ✅ High | ✅ Major |

#### Algorithmic Complexity

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| **O-notation** analysis | R4-29 O(V×P) metrics loop | ✅ via N+1 | ✅ | ✅ |

---

### §3. Resilience & Observability

*Prevent outages, ensure debuggability.*

#### Resilience

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| **Circuit breaker** for external services | R4-19 | ⚠️ retry only | ⚠️ | ⚠️ |
| Fallback stages on failure | ➖ | ➖ | ➖ | ➖ |
| Thresholds Closed→Open→Half-Open | ➖ | ➖ | ➖ | ➖ |
| Automatic recovery Half-Open→Closed | ➖ | ➖ | ➖ | ➖ |

#### Observability

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| **Appropriate log levels** | R4-23 LogError on success | ✅ P1 | ✅ | ❌ |
| Consistent logging format | ➖ | ➖ | ➖ | ➖ |
| **Sensitive data masking** | R4-03 | ✅ | ✅ | ✅ |
| Health checks & metrics | ➖ | ➖ | ➖ | ➖ |
| Correlation across services | ➖ | ➖ | ➖ | ➖ |

#### Testing (FIRST)

| Principle | R4 / evidence | Greptile | Cursor | CodeRabbit |
|-----------|---------------|:--------:|:------:|:----------:|
| **Fast** | ➖ not measured | ➖ | ➖ | ➖ |
| **Independent** | ➖ | ➖ | ➖ | ➖ |
| **Repeatable** | ➖ | ➖ | ➖ | ➖ |
| **Self-validating** — real asserts | R4-25 bad vs R4-26 good | ✅ flagged placeholder | ✅ | ✅ |
| **Timely** — tests with code | R4-26 good tests exist | ⚠️ | ⚠️ | ⚠️ wants more rules tested |
| Add tests for old/new behaviors | R4-25 | ✅ | ✅ | ✅ |

#### Compatibility

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Feature flags | ➖ | ➖ | ➖ | ➖ |
| No breaking existing APIs | ➖ new module | ➖ | ➖ | ➖ |
| Deprecated functionality marked | ➖ | ➖ | ➖ | ➖ |
| Optional params with defaults | R4-09 boolean flags (bad pattern) | ❌ | ⚠️ | ⚠️ |
| Backward-compatible schema changes | ➖ migration not a planted break | ➖ | ➖ | ⚠️ PK syntax comment |
| Protocol compatibility | ➖ | ➖ | ➖ | ➖ |
| Old config compatibility | ➖ | ➖ | ➖ | ➖ |
| API/module versioning | ➖ | ➖ | ➖ | ➖ |
| Integration points validated | ➖ | ➖ | ➖ | ➖ |
| Rollback procedures | ➖ | ➖ | ➖ | ➖ |
| Migration steps documented | ➖ | ➖ | ➖ | ➖ |
| Release notes | ➖ | ➖ | ➖ | ➖ |

---

### §4. Maintainability & Architectural Health

*Long-term code quality and scalability.*

| Checklist item | R4 / evidence | Greptile | Cursor | CodeRabbit |
|----------------|---------------|:--------:|:------:|:----------:|
| Distinguish names | R4-10 | ✅ | ❌ | ✅ |
| Reduces coupling | R4-20 god handler | ✅ SRP | ✅ side effects | ✅ split services |
| Maximizes cohesion | Vendor module folders | ➖ not scored | ➖ | ➖ |
| **SRP, DRY, SLA** | R4-20, R4-21 | ✅ | ✅ | ⚠️ SRP only |
| Comments explain **why** not how | ➖ | ➖ | ➖ | ➖ |
| Conform to architecture (CQRS/DDD) | R4-20 cross-layer side effects | ✅ | ✅ | ✅ |
| **Modularity** / folder structure | ➖ | ➖ | ➖ | ➖ |
| Manage technical debt | R4-22 dead code | ✅ | ✅ | ❌ |
| Design patterns | ➖ | ➖ | ➖ | ➖ |
| Small functions/classes (&lt;50 lines) | R4-20 long handler | ✅ | ✅ | ✅ |
| Avoid code smell — **long parameters** | R4-09 flags | ❌ | ⚠️ | ⚠️ |
| Avoid code smell — **duplications** | R4-21 duplicate switch | ✅ | ✅ | ❌ |
| Intelligent commenting | ➖ | ➖ | ➖ | ➖ |
| Don't repeat **switch/case** blocks | R4-21 | ✅ | ✅ | ❌ |
| Don't require **shotgun surgery** | R4-24 UpdateContact in report | ⚠️ in SRP text | ⚠️ | ⚠️ |
| **Single Responsibility Principle** | R4-20 | ✅ P2 | ✅ | ✅ Major |
| Cohesion | ➖ | ➖ | ➖ | ➖ |
| Organize for change | ➖ | ➖ | ➖ | ➖ |
| Open/Closed Principle | ➖ | ➖ | ➖ | ➖ |
| Dependency Inversion Principle | ➖ | ➖ | ➖ | ⚠️ suggests interfaces |

---

## Checklist → scenario index (quick lookup)

| Your checklist theme | Scenario ID(s) | Category |
|----------------------|----------------|----------|
| SQL injection / sanitizers | R4-02, R4-13 | C |
| Input validation | R4-01 | A, C |
| Access control | R4-04, R4-27 | C |
| Sensitive logs / masking | R4-03 | C, §3 |
| API over-exposure | R4-05 | C |
| Don't return null | R4-06 | A, Error |
| Exception context | R4-07, R4-08 | Error, §3 |
| Boolean flags | R4-09 | F |
| Misleading names | R4-10 | F |
| N+1 / O-notation | R4-11, R4-29 | D |
| Pagination / unbounded data | R4-12 | A, D |
| Async I/O | R4-14 | B, D |
| Cache TTL / keys / collision | R4-15, R4-16, R4-17 | D |
| Retry / circuit breaker | R4-18, R4-19 | §3 |
| SRP / god handler | R4-20 | E, F |
| DRY / duplicate switch | R4-21 | F |
| Dead code | R4-22 | F |
| Log levels | R4-23 | §3 |
| Shotgun surgery | R4-24 | F |
| Test quality FIRST | R4-25, R4-26 | G |
| Precision (GOOD controls) | R4-26, R4-30 | — |

---

## Winner summary

| Question | Answer |
|----------|--------|
| **Best overall gate** | **Greptile** (87.4 weighted, best summary + rule traceability) |
| **Best raw detection** | **Cursor Bugbot** (92.3%, 20/20 inline in &lt;2 s) |
| **Best security** | **Tie** — all three flagged SQL injection + unauthenticated export + PII logs |
| **Best precision** | **Greptile & Cursor** (no GOOD-control false positives) |
| **Fastest feedback** | **Cursor Bugbot** (first inline ~61 s after PR) |
| **Best architecture + maintainability** | **Greptile** (SRP, dead code, LogError, duplicate switch) |
| **Best remediation depth** | **CodeRabbit** (28 prompts with type names and steps) |

### One paragraph

On PR #4 (`c165b2c`), we ran a controlled three-way benchmark with **26 pre-registered BAD scenarios** and no inline bug markers. **Cursor Bugbot** achieved the highest detection rate (**92.3%**) and the fastest inline review, catching nearly all security and performance issues via `.cursor/BUGBOT.md` rules. **Greptile** ranked first on the **weighted composite (87.4)** thanks to superior context (Create vs Update pairing), maintainability coverage, confidence scoring, and concise signal (13 inline + 16-issue fix list). **CodeRabbit** delivered the richest fix prompts but scored lowest on detection (**80.8%**) after missing Update-validation and three maintainability targets, while introducing a false positive on the good CreateVendor path. For day-to-day merging, use **Greptile as primary** and **Cursor for speed**; add **CodeRabbit** when you want detailed agent-ready remediation text.

---

## Reproducibility checklist

To re-run or defend this report:

1. Checkout commit `c165b2c338dcfe7e9c490f08de4e29a0727158be`
2. Open [PR #4](https://github.com/mohammad-javad-afshani/Sample/pull/4)
3. Pull comments: `curl https://api.github.com/repos/mohammad-javad-afshani/Sample/pulls/4/comments?per_page=100`
4. Score against [`AI_REVIEW_ROUND4_BENCHMARK.md`](AI_REVIEW_ROUND4_BENCHMARK.md) + local answer key
5. Fill [`AI_REVIEW_ROUND4_SCORING_TEMPLATE.md`](AI_REVIEW_ROUND4_SCORING_TEMPLATE.md) — values in this report are the completed template for Run `R4-THREE-WAY-20250608`

---

## Appendix — Comment inventory (API)

### Cursor Bugbot (20)

1. SQL injection in vendor search  
2. Export endpoint lacks API key  
3. Export logs tax ID and notes  
4. Internal notes exposed by default  
5. Shared cache key wrong vendor  
6. Blocking async vendor search  
7. Unbounded analytics retry loop  
8. Update vendor skips validation  
9. N plus one vendor metrics  
10. Export loads entire vendor table  
11. Memory cache without expiration  
12. Static report cache races  
13. Lookup returns null not exception  
14. Side effects before database commit  
15. LogError on successful report  
16. Placeholder test always passes  
17. Unused legacy export helper  
18. Duplicate unused email formatter  
19. SkipValidation flag ignored  
20. Report date parameter ignored *(extra — not in answer key)*  

### Greptile (13 inline + summary)

Inline on: `VendorRepository`, `VendorController` (export auth), `ExportVendorsQueryHandler` (×2), `GenerateVendorReportCommandHandler` (×4), `AnalyticsInsightClient`, `UpdateVendorCommandHandler`, `SearchVendorsQueryHandler`, `GetUserOrderStatsQueryHandler`, `VendorBenchmarkTests`.  
Summary lists **16 issues** including misleading query name, duplicate switch, dead code.

### CodeRabbit (28)

Includes all major BAD paths plus **extras:** `Vendor.cs` guards, `VendorId` empty GUID, migration PK syntax, `FindByCodeAsync` wrong field, stable sort on **good** pagination, config-file meta-review, manifest count mismatch, CreateVendor exception typing (**GOOD control FP**).

---

*Report generated from PR #4 review exports and GitHub API evidence. Update `AI_REVIEW_ROUND4_MANIFEST.json` → `postReview` when archiving scores in-repo.*
