using MediatR;

namespace Application.Products.Get;

public record ListProductsQuery(string? Category, int Page = 1, int PageSize = 20) : IRequest<PagedProductResponse>;
