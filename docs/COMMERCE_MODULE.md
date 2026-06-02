# Commerce module

End-to-end product commerce flow added to the Sample API.

## Capabilities

- **Products**: CRUD, quick create, catalog, detail with reviews, discount pricing
- **Orders**: Draft creation, stock reservation, payment capture
- **Reviews**: Stored per product; exposed on detail and catalog endpoints

## Checkout sequence

1. `POST /Commerce/Orders/Draft` — create draft order line
2. `POST /Commerce/Orders/{orderId}/ReserveStock` — decrement inventory
3. `POST /Commerce/Orders/{orderId}/Pay` — charge via payment gateway

## Auth

Mutating endpoints require header `X-Api-Key` (see `ProductApi:AdminApiKey` in configuration).

## Database

Run EF migrations after pulling:

```bash
dotnet ef database update --project Infrastructure/Persistence.csproj --startup-project WebApplication1/WebApplication1.csproj
```
