using Domain.Products;
using MediatR;

namespace Application.Products.Get;

internal sealed class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, PagedProductResponse>
{
    private const int MaxPageSize = 100;

    private readonly IProductRepository _productRepository;

    public ListProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedProductResponse> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, MaxPageSize);

        var (items, totalCount) = await _productRepository.FindPagedAsync(
            request.Category,
            page,
            pageSize,
            cancellationToken);

        var responses = items.Select(p => new ProductResponse(
            p.Id.Value,
            p.Name,
            p.Description,
            p.Price,
            p.Category,
            p.StockQuantity,
            p.ViewCount)).ToList();

        return new PagedProductResponse(responses, totalCount, page, pageSize);
    }
}
