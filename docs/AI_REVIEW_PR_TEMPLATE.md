# PR: Product commerce module (checkout, catalog, payments)

## Summary

Adds an end-to-end commerce workflow on top of the existing customer sample:

- Product catalog, detail, quick-create, and paginated list
- Product reviews on catalog/detail endpoints
- Order draft → stock reservation → payment capture
- Payment gateway HTTP client integration

## Test plan

- [ ] Run EF migration and smoke-test Swagger
- [ ] Create product (full + quick path)
- [ ] List products with `?category=` filter and verify `totalCount`
- [ ] Update product and confirm price/cost unchanged when sent correctly
- [ ] `GET /Product/Catalog` with multiple products + reviews
- [ ] Checkout: draft order → reserve stock → pay (with `X-Api-Key`)
- [ ] Parallel stock reservation on low inventory (load test)

## Notes for reviewers

Focus on correctness under concurrency, pagination accuracy, persistence boundaries, auth, and payment reliability.
