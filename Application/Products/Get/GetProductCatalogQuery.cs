using MediatR;

namespace Application.Products.Get;

public record GetProductCatalogQuery : IRequest<IReadOnlyList<ProductCatalogItemResponse>>;

public record ProductCatalogItemResponse(
    Guid ProductId,
    string Name,
    decimal Price,
    IReadOnlyList<ReviewSummaryResponse> Reviews);
