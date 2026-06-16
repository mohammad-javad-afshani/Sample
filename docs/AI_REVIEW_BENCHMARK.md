# AI Code Review Benchmark — Round 1 + Round 2

**PR:** [Sample #1](https://github.com/mohammad-javad-afshani/Sample/pull/1)  
**Branch:** `test-code-review`  
**Purpose:** Measurable comparison of Greptile vs CodeRabbit on realistic .NET commerce code.

---

## What this PR contains

| Module | Description |
|--------|-------------|
| **Round 1 — Commerce** | Products, orders, stock, payments, catalog |
| **Round 2 — Promotions** | Coupons, webhook notifications after payment |
| **Round 3 — Refunds** | Refund request/process, inventory adjust, list endpoint |

Each round embeds intentional defects **without inline markers** so AI reviewers are not primed.

**Presentable report (all runs, metrics, evidence):** [`AI_CODE_REVIEW_FINAL_REPORT.md`](AI_CODE_REVIEW_FINAL_REPORT.md)  
**Run tracking:** [`AI_REVIEW_RUN_METRICS.md`](AI_REVIEW_RUN_METRICS.md) · [`AI_REVIEW_RUN_MANIFEST.json`](AI_REVIEW_RUN_MANIFEST.json)

---

## Scoring rubric (100 points)

| Category | Weight | Scenario IDs |
|----------|--------|--------------|
| Correctness | 30% | R1-1, R1-2, R1-3, R1-4, R2-12, R2-13 |
| Security | 20% | R1-5, R2-9, R2-10 |
| Performance | 10% | R1-7, R2-14 |
| Architecture / DDD | 15% | R1-6, R1-4, R2-12 |
| Reliability / Distributed | 20% | R1-8, R2-11, R2-15 |
| Noise penalty | 5% | false positives subtract |

**Per scenario:** Found = 100%, Partial = 50%, Missed = 0%  
**Score** = Σ (category_weight × found_ratio_in_category) − noise

---

## Round 1 scenarios (commerce)

| ID | What to look for | Primary file |
|----|------------------|--------------|
| R1-1 | Price / InternalCost parameter swap | `UpdateProductCommandHandler.cs` |
| R1-2 | Pagination totalCount ignores category filter | `ProductRepository.cs` |
| R1-3 | Order draft never saved (no SaveChanges) | `CreateOrderDraftCommandHandler.cs` |
| R1-4 | Stock decrement bypasses domain + concurrency | `ReserveStockCommandHandler.cs` |
| R1-5 | API key compared with `==` (timing side-channel) | `ProductController`, `CommerceController` |
| R1-6 | QuickCreate skips name/price validation | `Product.cs`, `QuickCreateProductCommandHandler.cs` |
| R1-7 | N+1 loading reviews in catalog | `GetProductCatalogQueryHandler.cs` |
| R1-8 | Payment not idempotent; pending not persisted before gateway | `ProcessPaymentCommandHandler.cs` |

---

## Round 2 scenarios (promotions + webhooks)

| ID | What to look for | Primary file |
|----|------------------|--------------|
| R2-9 | SQL injection in coupon search (`FromSqlRaw` + interpolation) | `CouponRepository.cs` |
| R2-10 | Hardcoded webhook HMAC signing key | `WebhookNotificationClient.cs` |
| R2-11 | Webhook dispatched **before** DB commit (no outbox) | `ProcessPaymentCommandHandler.cs` |
| R2-12 | Coupon usage limit race (check-then-act, no lock/version) | `ApplyCouponCommandHandler.cs` |
| R2-13 | Discount applied twice (stacked percent calculation) | `ApplyCouponCommandHandler.cs` |
| R2-14 | CancellationToken ignored (`Task.Run` + `.GetResult()`) | `SearchCouponsQueryHandler.cs` |
| R2-15 | Fire-and-forget webhook (`Task.Run`, no await, errors swallowed) | `WebhookNotificationClient.cs` |

**Total: 15 scenarios** (R2-14 and R2-15 are separate performance/reliability signals)

---

## Round 3 scenarios (refunds — Greptile Strict run)

| ID | What to look for | Primary file |
|----|------------------|--------------|
| R3-16 | Unauthenticated list of all refunds | `RefundController.cs` — `GET List` |
| R3-17 | Sensitive financial data in logs | `RefundGatewayClient.cs` |
| R3-18 | Refund amount via float cast | `RequestRefundCommandHandler.cs` |
| R3-19 | Inventory adjust bypasses domain | `AdjustInventoryCommandHandler.cs` |
| R3-20 | Empty catch on refund gateway failure | `ProcessRefundCommandHandler.cs` |
| R3-21 | Request refund never persisted | `RequestRefundCommandHandler.cs` |

**Total cumulative: 21 scenarios**

---

## Expected tool strengths (hypothesis)

| Signal type | Greptile | CodeRabbit |
|-------------|----------|------------|
| SaveChanges / UoW | Strong | Medium |
| Domain bypass | Strong | Medium |
| SQL injection | Medium | Strong |
| Idempotency / payment | Medium | Strong |
| Webhook ordering | Medium | Strong |
| N+1 / pagination subtle | Weak | Weak |
| Parameter swap | Medium | Medium |

---

## Workflow after push (Run B — Greptile Strict + Round 3)

1. Commit Round 3 (`feat: refunds module — benchmark round 3`)
2. Run `./scripts/capture-ai-review-push.sh` (ثبت commit در manifest)
3. Push branch `test-code-review-2` (همان PR #2)
4. PR body از [`AI_REVIEW_PR_TEMPLATE.md`](AI_REVIEW_PR_TEMPLATE.md)
5. Wait for Greptile (strict) + CodeRabbit
6. Fill [`AI_REVIEW_RUN_METRICS.md`](AI_REVIEW_RUN_METRICS.md) و/یا export + لینک PR
7. Compare با [`AI_REVIEW_RUN_MANIFEST.json`](AI_REVIEW_RUN_MANIFEST.json) و answer key در `docs/internal/`
8. Update [`AI_CODE_REVIEW_BENCHMARK_FULL_REPORT.md`](AI_CODE_REVIEW_BENCHMARK_FULL_REPORT.md) — بخش Run B

---

## Tool configuration

**Tool configuration (Greptile + Cursor Bugbot + CodeRabbit):** [`AI_REVIEW_TOOL_CONFIG.md`](AI_REVIEW_TOOL_CONFIG.md)  
**Shared review context:** [`REVIEW_PROJECT_CONTEXT.md`](REVIEW_PROJECT_CONTEXT.md)

---

## PR body template

Use [`AI_REVIEW_PR_TEMPLATE.md`](AI_REVIEW_PR_TEMPLATE.md) when updating the PR description.
