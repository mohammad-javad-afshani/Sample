using Domain.Products;
using MediatR;

namespace Application.Products.Get;

public record GetProductQuery(ProductId ProductId) : IRequest<ProductResponse?>;

public record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    decimal InternalCost,
    string Category,
    int StockQuantity,
    int ViewCount);
