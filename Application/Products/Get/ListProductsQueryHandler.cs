using Application.Products;
using Domain.Products;
using MediatR;

namespace Application.Products.Get;

internal sealed class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, PagedProductResponse>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IProductRepository _productRepository;

    public ListProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedProductResponse> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? DefaultPageSize
            : Math.Min(request.PageSize, MaxPageSize);

        var (items, totalCount) = await _productRepository.FindPagedAsync(
            request.Category,
            page,
            pageSize,
            cancellationToken);

        var responses = items.Select(ProductResponseMapper.ToResponse).ToList();

        return new PagedProductResponse(responses, totalCount, page, pageSize);
    }
}
