using Domain.Products;
using MediatR;

namespace Application.Products.Get;

public record GetProductQuery(ProductId ProductId) : IRequest<ProductResponse>;

public record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    int StockQuantity,
    int ViewCount);

public record PagedProductResponse(
    IReadOnlyList<ProductResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
