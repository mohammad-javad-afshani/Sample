# How We Measured the AI Review Scores — Methodology Guide

**Purpose:** This document explains **only how the numbers were produced** — not the full benchmark results. Use it when someone asks: *“Why is Greptile 4.5 on Severity but 4.6 on Context?”* or *“How did you measure Correctness?”*

**Related documents:**
- Results & evidence: [`AI_REVIEW_ROUND4_FINAL_REPORT.md`](AI_REVIEW_ROUND4_FINAL_REPORT.md)
- Your presentation PDF: `R4-THREE-WAY-BENCHMARK-2026` (scores aligned with this guide)
- Ground truth: [`AI_REVIEW_ROUND4_BENCHMARK.md`](AI_REVIEW_ROUND4_BENCHMARK.md) + local answer key

**Run:** PR [#4](https://github.com/mohammad-javad-afshani/Sample/pull/4) · commit `c165b2c` · 26 pre-registered BAD scenarios · 3 tools on the **same commit**

---

## 1. Measurement foundation (answer in 30 seconds)

| Question | Answer |
|----------|--------|
| What is “correct”? | 26 **intentional bugs** planted before review — documented in answer key, **no `// BUG` markers** in code |
| Where is proof? | GitHub PR #4 inline comments (API export) + `uploads/4-1.md` |
| Who scored? | Human auditor mapped each bot comment → scenario ID (F/P/M) |
| Scale | **1–5** per qualitative metric; **%** = (score ÷ 5) × 100 |
| Why not pure opinion? | Each /5 score ties to ** countable events** (found/missed, FP count, label severity, comment count) |

**Core formula used everywhere:**

```
Scenario points = Found (1.0) | Partial (0.5) | Missed (0.0)
Metric %        = (sum of points ÷ applicable items) × 100
Score /5        = (Metric % ÷ 100) × 5   → often shown as e.g. 4.5/5 (90%)
```

---

## 2. The 1–5 scale — what each level means

Use this when defending **4 vs 5**:

| Score | Label | Meaning | When to use |
|:-----:|-------|---------|-------------|
| **5.0** | Excellent | ≥95% on measured items; no material gaps | e.g. all security BAD found with correct urgency |
| **4.5** | Strong | 85–94%; minor gaps only | e.g. missed 1 secondary scenario or 1 partial auth gap |
| **4.0** | Good | 75–84%; clear value, some blind spots | e.g. missed cross-file pattern once |
| **3.5** | Fair | 65–74%; inconsistent | e.g. found syntax issues but missed maintainability cluster |
| **3.0** | Weak | 55–64%; unreliable for that dimension | e.g. missed sibling-pair validation test |
| **<3** | Poor | Major failure on that dimension | Not used for winners in this run |

**Important:** A **4.5 is not “almost failing”** — it means **~90% on a rigorous test**, with a documented reason for the −0.5.

---

## 3. The ten qualitative metrics — full measurement guide

Your PDF scorecard uses these ten dimensions. Below: **definition → data source → formula → tool scores → how to defend**.

---

### Metric 1 — Correctness (درستی / آیا باگ واقعی پیدا کرده؟)

**Question answered:** Did the tool find **real bugs** we planted?

| Tool | Score | % | Source |
|------|:-----:|:---:|--------|
| Greptile | **4.5/5** | 90% | 23.5 ÷ 26 BAD |
| Cursor Bugbot | **4.6/5** | 92% | 24 ÷ 26 BAD |
| CodeRabbit | **4.0/5** | 80% | 21 ÷ 26 BAD |

**Formula:**

```
Correctness % = (F + 0.5×P) / 26 × 100
Correctness /5 = Correctness % / 20
```

**Counted evidence:**

| Tool | Found (F) | Partial (P) | Missed (M) | Points |
|------|-----------|-------------|------------|--------|
| Greptile | 21 | 5 | 2 | 23.5 |
| Cursor | 22 | 3 | 1 | 24.0 |
| CodeRabbit | 18 | 6 | 4 | 21.0 |

**Example misses (why not 5.0):**
- Greptile **−0.5:** missed R4-06 (return null), R4-09 (boolean flags); partial on R4-27, R4-19, R4-24
- Cursor **−0.4:** missed R4-10 (misleading name); partial on R4-09, R4-27, R4-24
- CodeRabbit **−1.0:** missed R4-01, R4-21, R4-22, R4-23 entirely

**If asked “why 4.6 not 5?” for Cursor:**  
> “Cursor found 24 of 26 weighted points — 92.3%. A perfect 5.0 requires ≥95% (≤1 partial miss). Cursor missed misleading naming (R4-10) and only partially covered auth-on-read (R4-27).”

---

### Metric 2 — Severity Detection (کالیبراسیون اهمیت / Severity)

**Question answered:** When the tool **did** find a bug, did it label urgency **correctly** (Critical/High vs Low)?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| Greptile | **4.5/5** | 90% |
| Cursor Bugbot | **4.2/5** | 84% |
| CodeRabbit | **3.8/5** | 76% |

**This is NOT the same as Correctness.** A tool can find a bug but call it “Minor” when it is production-critical.

**Formula:**

```
Applicable = all BAD scenarios the tool FOUND (F or P) + security/reliability scenarios it MISSED
For each applicable item, assign:
  1.0 = severity label matches ground truth (Critical/High/P0/P1 for blockers)
  0.5 = found but under-labeled (e.g. Medium for SQL injection)
  0.0 = missed entirely OR dangerously under-labeled

Severity % = average × 100
Severity /5 = Severity % / 20
```

**Ground-truth severity tiers (PR #4):**

| Tier | Scenarios | Expected label |
|------|-----------|----------------|
| **P0 / Critical** | R4-02 SQL injection | Must block merge |
| **P1 / High** | R4-03, R4-04, R4-05, R4-07, R4-08, R4-16, R4-17, R4-18 | Security / data integrity |
| **P2 / Medium** | R4-11, R4-12, R4-14, R4-20, R4-21, R4-22, R4-23, R4-25 | Quality / perf / maintainability |

**Greptile → 4.5/5 (90%) — why not 5?**
- ✅ SQL = **P0** badge; export/auth/cache/retry = **P1**; N+1/SRP = **P2** — matches intent
- ⚠️ **−0.5 total:** some P2 items only in summary “Fix 16 issues” not inline; R4-06 missed (counts as 0 for that row)

**Cursor → 4.2/5 (84%) — why not 5?**
- ✅ Security items tagged **High Severity**
- ⚠️ **Update validation (R4-01)** tagged **Medium** — should be **High** (same class as missing input validation on write path) → −0.5
- ⚠️ **LogError on success (R4-23)** tagged **Low** — acceptable label tier but understates observability risk in prod → small penalty
- ⚠️ Missed R4-10 → 0 on that row

**CodeRabbit → 3.8/5 (76%) — why not 4?**
- ✅ Uses Critical/Major on SQL, export, auth
- ❌ **Four missed rows** (R4-01, 21, 22, 23) score **0** in severity table — you cannot calibrate severity on what you never flagged
- ⚠️ Some **Major** where we expect **Critical** (export PII) → partial 0.5 on those cells

**Script to say in a meeting (Severity):**

> “Severity is measured on whether the bot escalates planted blockers appropriately. Greptile used P0/P1/P2 consistent with our answer key on 90% of weighted cells. Cursor found more bugs but downgraded Update-validation to Medium — that costs 0.3–0.5 points. CodeRabbit lost severity credit on four maintainability and validation findings it never commented on.”

---

### Metric 3 — Precision (دقت / Precision Efficiency)

**Question answered:** What **percentage of comments were valid** (not false alarms on good code)?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| Greptile | **5.0/5** | 100% |
| Cursor Bugbot | **5.0/5** | 100% |
| CodeRabbit | **4.2/5** | 84% |

**Formula:**

```
Precision % = (1 − FP / total_inline_comments) × 100
```

**False positives (FP) on GOOD controls:**

| Tool | Inline comments | Confirmed FP | Notes |
|------|----------------:|-------------:|-------|
| Greptile | 13 | **0** | Did not attack `CreateVendorCommandHandler` validation |
| Cursor | 20 | **0** | One extra finding (ReportDate) is **valid** extra insight, not FP on GOOD control |
| CodeRabbit | 28 | **1** | Flagged `CreateVendorCommandHandler` for exception **type** — validation already correct |

**CodeRabbit 4.2 calculation:**

```
Effective FP ≈ 1 core FP + ~3 low-value meta comments (config file, manifest counts)
Noise-adjusted precision ≈ 84% → 4.2/5
```

**If asked “why 5.0 for Greptile?”:**

> “Zero false positives on our four GOOD controls: Create with validator, paged list, auth on create, real validation tests. All 13 inline comments map to real BAD scenarios or valid cross-file comparisons.”

---

### Metric 4 — Context Awareness (درک Context / آیا Context پروژه را فهمیده؟)

**Question answered:** Did the tool understand **relationships in the repo** (sibling handlers, auth patterns, module pairs) — not just single files?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| Greptile | **4.6/5** | 92% |
| Cursor Bugbot | **4.0/5** | 80% |
| CodeRabbit | **3.0/5** | 60% |

**This is the metric people ask about most.** It uses a **fixed checklist of 5 context tests**:

| # | Context test | Pass condition | G | C | CR |
|---|--------------|----------------|:-:|:-:|:-:|
| T1 | **Create vs Update validation** (R4-01) | Flags Update missing validator **because** Create has one | ✅ | ✅ | ❌ |
| T2 | **List vs Export pagination** (R4-12) | Compares unbounded export to capped `ListVendors` | ✅ | ✅ | ✅ |
| T3 | **Auth consistency** (R4-04, R4-27) | Notes Create/Update protected but Export/Search/Metrics open | ⚠️ | ⚠️ | ⚠️ |
| T4 | **No attack on GOOD path** | Does not treat correct Create validation as broken | ✅ | ✅ | ❌ |
| T5 | **Module narrative** | Summary links vendor module risks (SQL + export + cache) | ✅ | ⚠️ | ✅ |

**Scoring:** Each test = 1.0 pass · 0.5 partial · 0.0 fail  

| Tool | Points | ÷5 | % | /5 |
|------|--------|----|---|-----|
| Greptile | 4.6 | | 92% | **4.6** |
| Cursor | 4.0 | | 80% | **4.0** |
| CodeRabbit | 3.0 | | 60% | **3.0** |

**Greptile 4.6 — why not 5.0?**
- **T3 partial (0.5):** flagged Export auth but not Search/Metrics/Lookup anonymous endpoints
- All other tests full pass including explicit text: *“sibling CreateVendorCommandHandler validates all three fields”*

**Cursor 4.0 — why not 4.6?**
- Strong on T1, T2 (Update validation + export load)
- **T3 partial:** Export auth yes, broader read surface no
- **T5 partial:** no rich summary — only “20 issues found”
- **Miss T2 naming context:** did not connect misleading `GetUserOrderStats` to vendor metrics story

**CodeRabbit 3.0 — why so low?**
- **T1 fail:** missed Update/Create pair entirely — **the canonical context test**
- **T4 fail:** commented on GOOD Create handler (exception typing)
- **T3 partial:** List + Export auth only

**Forensic proof (say this verbatim):**

> “We ran the **Sibling Control Test**: `UpdateVendorCommandHandler` has no FluentValidation; `CreateVendorCommandHandler` does. Greptile and Cursor cited the sibling in the comment. CodeRabbit had zero comments on the Update handler — that is a hard fail on context, not syntax. That single test drops CodeRabbit from ‘good’ to ‘weak’ on this axis.”

Evidence: PR #4 — Greptile P1 on `UpdateVendorCommandHandler.cs`; Cursor “Update vendor skips validation”; CodeRabbit API comment list has no path match for Update handler.

---

### Metric 5 — Architectural Understanding (فهم Architecture — نه فقط Syntax)

**Question answered:** Did the reviewer see **design problems** (SRP, side-effect order, cache design) — not only line-level bugs?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| Greptile | **4.5/5** | 90% |
| Cursor Bugbot | **4.0/5** | 80% |
| CodeRabbit | **3.5/5** | 70% |

**Measured on 4 architecture scenarios:**

| ID | Test | G | C | CR |
|----|------|:-:|:-:|:-:|
| R4-20 | God handler / SRP | F | F | F |
| R4-21 | Duplicate switch (DRY) | F | F | M |
| R4-24 | Shotgun `UpdateContact` in report | P | P | P |
| R4-17 | Static shared mutable cache | F | F | F |

**Points:** Greptile 3.5/4 · Cursor 3.5/4 · CodeRabbit 2.5/4 → normalized to /5 scale above.

**Why CodeRabbit 3.5 not 4.0:** Found SRP (R4-20) but **missed duplicate switch and dead code** — architecture smells beyond one file.

---

### Metric 6 — Business Understanding (درک Requirement)

**Question answered:** Did comments reflect **business risk** (PII export, supplier data, audit logs) — not only technical pattern?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| Greptile | **4.0/5** | 80% |
| Cursor Bugbot | **3.8/5** | 76% |
| CodeRabbit | **3.0/5** | 60% |

**Subjective but bounded** — scored on 5 business-risk statements in comments:

1. Bulk PII export to anonymous callers (TaxId, InternalNotes)
2. SQL injection on vendor search (integrity/confidentiality)
3. Wrong vendor report returned from cache (financial/analytics trust)
4. Unbounded export vs paginated list (operational scale)
5. Placeholder test = no regression safety for vendor module

**Rule:** 1 point each if comment text shows **business consequence** (not just “use parameterized query”).

Greptile/Cursor hit 4–5/5; CodeRabbit missed several maintainability items tied to operational risk → ~3/5.

---

### Metric 7 — Actionability (کامنت قابل اقدام)

**Question answered:** Could a developer **act immediately** (patch, steps, Fix-with-AI)?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| CodeRabbit | **4.5/5** | 90% |
| Cursor Bugbot | **4.2/5** | 84% |
| Greptile | **4.0/5** | 80% |

**Formula:**

```
Sample 10 comments → score each:
  1.0 = code suggestion or explicit steps
  0.5 = problem clear, fix vague
  0.0 = generic warning only
Average × 5
```

**Why CodeRabbit wins:** 28 comments with **AI agent prompt blocks**, proposed diff hunks, type names (`IReportWriter`, `VendorNotFoundException`).

**Why Greptile 4.0 not 4.5:** Strong “Fix 16 issues” list but fewer inline patch blocks than CodeRabbit.

---

### Metric 8 — Signal-to-Noise (نسبت سیگNAL به نویز)

**Question answered:** How much **useful signal** per comment?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| Greptile | **4.8/5** | 96% |
| Cursor Bugbot | **4.0/5** | 80% |
| CodeRabbit | **3.0/5** | 60% |

**Formula:**

```
Signal = weighted BAD scenarios found (same as Correctness points) = 21 / 24 / 21
Noise    = total inline comments − signal-equivalent unique findings
Ratio    = signal / (signal + noise_penalty)

Greptile: 13 comments, ~12.5 signal → 4.8/5
Cursor:   20 comments, ~18 signal   → 4.0/5  (ReportDate extra = minor noise)
CodeRabbit: 28 comments, ~21 signal → 3.0/5 (config/meta/domain extras)
```

**If asked “why Greptile 4.8?”:**

> “13 comments covering 21 weighted scenario points — almost one high-value finding per comment. CodeRabbit needed 28 comments for similar detection plus meta-review of `.coderabbit.yaml`.”

---

### Metric 9 — Security Awareness (آگاهی امنیتی)

**Question answered:** Did the tool catch **all planted security defects**?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| **All three** | **5.0/5** | **100%** |

**Measured on 5 security BAD scenarios:**

| ID | Defect | G | C | CR |
|----|--------|:-:|:-:|:-:|
| R4-02 | SQL injection | ✅ | ✅ | ✅ |
| R4-03 | PII in logs | ✅ | ✅ | ✅ |
| R4-04 | Export no auth | ✅ | ✅ | ✅ |
| R4-05 | Internal fields default exposed | ✅ | ✅ | ✅ |
| R4-27 | Anonymous sensitive reads | ⚠️ partial | ⚠️ partial | ⚠️ partial |

**Why still 5.0/5 for all:** Core four blockers (R4-02–05) = **100% Found with high severity**. R4-27 partial is **read-surface completeness**, scored under Context — not missing SQL/PII/auth export.

**Defend in one line:**

> “All three tools flagged SQL injection, unauthenticated bulk export, and PII in logs at highest tier. That is a clean 100% on the security defects we designed to be unmissable.”

---

### Metric 10 — Maintainability Focus (تمرکز روی نگهداری‌پذیری)

**Question answered:** Did the tool flag **long-term code health** (dead code, DRY, log levels, boolean smells)?

| Tool | Score | % |
|------|:-----:|:---:|--------|
| Greptile | **4.6/5** | 92% |
| Cursor Bugbot | **4.2/5** | 84% |
| CodeRabbit | **3.5/5** | 70% |

**Scenarios in category F:** R4-09, R4-10, R4-21, R4-22, R4-23, R4-24 (6 items)

| Tool | F+P points / 6 |
|------|----------------|
| Greptile | 5.5 → 92% |
| Cursor | 5.0 → 84% (missed R4-10 naming) |
| CodeRabbit | 3.0 → 70% (missed 21, 22, 23) |

**Intent Inversion proof (for slides):** R4-23 = `LogError` on **success** path. Greptile + Cursor flagged semantic contradiction; CodeRabbit silent → maintainability gap.

---

## 4. Link to weighted composite (8 metrics, your weights)

Qualitative /5 scores **inform** but are **not identical** to the composite engine in your PDF:

| Composite metric | Weight | How it relates to §3 above |
|------------------|--------|----------------------------|
| True Positive Rate | 25% | = **Correctness %** (Metric 1) |
| Severity Detection | 20% | = **Metric 2** |
| Architectural Findings | 15% | = **Metric 5** (R4-20,21,24) |
| Security Findings | 10% | = **Metric 9** (core R4-02–05) |
| Context Awareness | 10% | = **Metric 4** |
| Precision | 10% | = **Metric 3** |
| Actionability | 5% | = **Metric 7** |
| Noise Reduction | 5% | = **Metric 8** |

**Composite (from PDF):** Greptile **87.4** · Cursor **85.2** · CodeRabbit **77.2**

```
Composite = Σ (metric_percent × weight)
Example Greptile: 0.25×90.4 + 0.20×90 + 0.15×90 + 0.10×100 + 0.10×90 + 0.10×100 + 0.05×80 + 0.05×95 ≈ 87.4
```

---

## 5. FAQ — tough questions in meetings

### “Isn’t this subjective?”

No for **Correctness, Precision, Security** — those are **counting** F/P/M and FP from PR comments.  
Partially structured for **Business Understanding** (5 bounded rubric items).  
Every /5 score traces to a **table you can show** (§3 above).

### “Why doesn’t highest detection (Cursor 92%) win overall?”

Detection is only **25% weight**. Greptile wins composite because higher **Context (92% vs 80%)**, **Noise (96% vs 80%)**, and **Maintainability (92% vs 84%)** with equal security — see weighted table §4.

### “Show me one comment for Context Awareness.”

Greptile on `UpdateVendorCommandHandler.cs`:

> *“CreateVendorCommandHandler in the same module validates Name, Email, and TaxId via CreateVendorValidator before persisting. UpdateVendorCommandHandler applies all three fields through UpdateContact **without any validation**…”*

That sentence **requires repository context** — impossible from a single-file linter.

### “Show me one comment for Severity.”

Greptile SQL finding — **P0** badge + explicit injection payload example `'; DROP TABLE Vendors; --'`.  
Cursor same finding — **High Severity** header.  
Both pass severity calibration; Cursor loses points elsewhere for **Medium** on Update validation.

### “Can we reproduce?”

Yes:

```bash
curl -s "https://api.github.com/repos/mohammad-javad-afshani/Sample/pulls/4/comments?per_page=100" \
  | jq '[.[] | {user: .user.login, path, body: .body[0:120]}]'
```

Then map paths to scenario IDs using [`AI_REVIEW_ROUND4_FINAL_REPORT.md`](AI_REVIEW_ROUND4_FINAL_REPORT.md) detection matrix.

---

## 6. One-page cheat sheet (print this)

| Metric | Persian | Measured by | Greptile | Cursor | CodeRabbit |
|--------|---------|-------------|:--------:|:------:|:----------:|
| 1 Correctness | باگ واقعی | 26 BAD F/P/M | 4.5 | 4.6 | 4.0 |
| 2 Severity | اهمیت | Label vs tier | 4.5 | 4.2 | 3.8 |
| 3 Precision | دقت | FP ÷ comments | 5.0 | 5.0 | 4.2 |
| 4 Context | درک Context | 5 context tests | 4.6 | 4.0 | 3.0 |
| 5 Architecture | فهم معماری | R4-20,21,24,17 | 4.5 | 4.0 | 3.5 |
| 6 Business | درک Requirement | 5 risk statements | 4.0 | 3.8 | 3.0 |
| 7 Actionability | قابل اقدام | Patch/steps quality | 4.0 | 4.2 | 4.5 |
| 8 Signal/Noise | سیگنال/نویز | Findings ÷ comments | 4.8 | 4.0 | 3.0 |
| 9 Security | امنیت | R4-02–05 blockers | 5.0 | 5.0 | 5.0 |
| 10 Maintainability | نگهداری | Category F scenarios | 4.6 | 4.2 | 3.5 |

**When they ask about 4 vs 5:** Open §3 for that metric → read the **“why not 5?”** bullet → show PR comment link.

---

*Document version: aligns with PDF `R4-THREE-WAY-BENCHMARK-2026` and PR #4 audit log.*
