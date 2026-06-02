# PR: Commerce + Promotions (AI review benchmark)

## Summary

Extends the Sample API with a full checkout flow and promotion/webhook layer:

- Product catalog, detail, quick-create, paginated list
- Order draft → stock reservation → payment capture
- **New:** Coupon search & apply on draft orders
- **New:** Payment-completed webhook dispatch
- **New (Round 3):** Refund request/process, inventory adjustment API

## Test plan

- [ ] Run EF migrations (`AddCommerceModule`, `AddPromotionsModule`, `AddRefundsModule`)
- [ ] Create product; list with `?category=` and verify `totalCount`
- [ ] Checkout: draft → reserve stock → pay (header `X-Api-Key`)
- [ ] Search coupons: `GET /Promotion/Coupons/Search?term=SUMMER`
- [ ] Apply coupon on draft order: `POST /Promotion/Orders/{id}/ApplyCoupon`
- [ ] Verify webhook config in `Webhooks:BaseUrl`
- [ ] `POST /Refund/Request` then `POST /Refund/{id}/Process` (with `X-Api-Key`)
- [ ] `GET /Refund/List` — verify auth expectations
- [ ] `POST /Refund/Inventory/Adjust`

## Notes for reviewers

Please focus on:

- Transaction boundaries and `SaveChangesAsync`
- Domain invariants vs direct field mutation
- Security: SQL construction, secrets, auth comparison
- Performance: query shape, N+1, cancellation tokens
- Reliability: idempotency, webhook vs DB ordering, fire-and-forget HTTP

This PR is part of an **AI code review benchmark** (Run ID: `R3-GREPTILE-STRICT`) — treat it as production-critical commerce code. Greptile is on **strict** review mode.
