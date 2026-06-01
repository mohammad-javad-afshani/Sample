using Application.Data;
using Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Get;

internal sealed class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, List<ProductResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IProductRepository _productRepository;

    public ListProductsQueryHandler(IApplicationDbContext context, IProductRepository productRepository)
    {
        _context = context;
        _productRepository = productRepository;
    }

    public async Task<List<ProductResponse>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.Category))
        {
            var filtered = _productRepository.FindByCategoryAsync(request.Category).Result;
            return filtered.Select(p => new ProductResponse(
                p.Id.Value, p.Name, p.Description, p.Price, p.InternalCost,
                p.Category, p.StockQuantity, p.ViewCount)).ToList();
        }

        return await _context.Products
            .Select(p => new ProductResponse(
                p.Id.Value,
                p.Name,
                p.Description,
                p.Price,
                p.InternalCost,
                p.Category,
                p.StockQuantity,
                p.ViewCount))
            .ToListAsync(cancellationToken);
    }
}
