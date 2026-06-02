using Domain.Products;
using MediatR;

namespace Application.Products.Get;

public record GetProductDetailQuery(ProductId ProductId) : IRequest<ProductDetailResponse>;

public record ProductDetailResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    int StockQuantity,
    int ViewCount,
    IReadOnlyList<ReviewSummaryResponse> Reviews);

public record ReviewSummaryResponse(
    Guid Id,
    string Author,
    int Rating,
    string Comment,
    DateTime CreatedAtUtc);
