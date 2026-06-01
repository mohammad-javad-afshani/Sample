using MediatR;

namespace Application.Products.Get;

public record ListProductsQuery(string? Category) : IRequest<List<ProductResponse>>;
