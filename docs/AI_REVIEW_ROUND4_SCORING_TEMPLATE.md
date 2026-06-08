# Round 4 — Three-Way Review Scoring Template

**Run ID:** `R4-THREE-WAY-20250608`  
**PR URL:** ___________________________  
**Commit:** ___________________________  
**Date:** ___________________________

**Tools:** Greptile · Cursor · CodeRabbit

---

## 1. Detection matrix (26 BAD scenarios)

Score: **F** = Found · **P** = Partial · **M** = Missed · **FP** = False positive on GOOD control

| ID | Scenario | Greptile | Cursor | CodeRabbit | Evidence link |
|----|----------|----------|--------|------------|---------------|
| R4-01 | Update no validation | | | | |
| R4-02 | SQL injection search | | | | |
| R4-03 | PII in export logs | | | | |
| R4-04 | Export no auth | | | | |
| R4-05 | Internal fields exposed | | | | |
| R4-06 | Return null lookup | | | | |
| R4-07 | Generic exception | | | | |
| R4-08 | Swallowed exception retry | | | | |
| R4-09 | Boolean export flags | | | | |
| R4-10 | Misleading query name | | | | |
| R4-11 | N+1 metrics | | | | |
| R4-12 | Unbounded export | | | | |
| R4-13 | SELECT * | | | | |
| R4-14 | Sync-over-async search | | | | |
| R4-15 | Cache no TTL | | | | |
| R4-16 | Cache key collision | | | | |
| R4-17 | Static mutable cache | | | | |
| R4-18 | Unbounded retry loop | | | | |
| R4-19 | No circuit breaker | | | | |
| R4-20 | God handler report | | | | |
| R4-21 | Duplicate switch | | | | |
| R4-22 | Dead code | | | | |
| R4-23 | LogError success path | | | | |
| R4-24 | Shotgun UpdateContact | | | | |
| R4-25 | Placeholder test | | | | |
| R4-27 | Anonymous sensitive reads | | | | |
| R4-29 | O(V×P) metrics loop | | | | |

**Detection rate:** ___/26 = ___%

| Tool | Found | Partial | Missed |
|------|-------|---------|--------|
| Greptile | | | |
| Cursor | | | |
| CodeRabbit | | | |

---

## 2. GOOD controls — false positive check (precision)

| Control | File | Greptile FP? | Cursor FP? | CodeRabbit FP? |
|---------|------|--------------|------------|----------------|
| Create validation | `CreateVendorCommandHandler` | | | |
| Paged list | `ListVendorsQueryHandler` | | | |
| Auth on create | `VendorController.Create` | | | |
| Good tests | `VendorCreateValidationTests` | | | |

**FP count:** G ___ · C ___ · CR ___  
**Precision:** 1 − FP/total_comments = ___

---

## 3. Weighted composite (your model)

| Metric | Weight | Greptile | Cursor | CodeRabbit | Notes |
|--------|--------|----------|--------|------------|-------|
| True Positive Rate | 25% | | | | /26 BAD |
| Severity Detection | 20% | | | | P0/P1/Critical share |
| Architectural Findings | 15% | | | | R4-20–24 |
| Security Findings | 10% | | | | R4-02–05, 27 |
| Context Awareness | 10% | | | | Qualitative 0–100 |
| Precision | 10% | | | | §2 |
| Actionability | 5% | | | | Has fix/patch |
| Noise Reduction | 5% | | | | Lower comments = better |
| **TOTAL** | **100%** | | | | |

---

## 4. Qualitative rubric (Persian labels)

| متریک | Greptile | Cursor | CodeRabbit |
|--------|----------|--------|------------|
| Correctness — باگ واقعی | /5 | /5 | /5 |
| Severity — اهمیت | /5 | /5 | /5 |
| Precision — درصد درست | /5 | /5 | /5 |
| Context Awareness | /5 | /5 | /5 |
| Architectural Understanding | /5 | /5 | /5 |
| Business Understanding | /5 | /5 | /5 |
| Actionability | /5 | /5 | /5 |
| Signal-to-Noise | /5 | /5 | /5 |
| Security Awareness | /5 | /5 | /5 |
| Maintainability focus | /5 | /5 | /5 |

---

## 5. Operational metrics

| Event | Greptile | Cursor | CodeRabbit |
|-------|----------|--------|------------|
| First comment (time) | | | |
| Total inline comments | | | |
| Summary provided? | | | |
| Confidence / severity model | | | |

---

## 6. Winner summary (fill after scoring)

**Best overall gate:** ___________  
**Best security:** ___________  
**Best precision:** ___________  
**Fastest feedback:** ___________  
**Best for architecture:** ___________

**One paragraph:** …
