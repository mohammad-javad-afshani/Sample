using Application.Data;
using Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Get;

internal sealed class GetProductCatalogQueryHandler : IRequestHandler<GetProductCatalogQuery, IReadOnlyList<ProductCatalogItemResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCatalogQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductCatalogItemResponse>> Handle(
        GetProductCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var productIds = products.Select(p => p.Id).ToList();

        var reviewsByProduct = await _context.Reviews
            .AsNoTracking()
            .Where(r => productIds.Contains(r.ProductId))
            .Select(r => new
            {
                r.ProductId,
                Review = new ReviewSummaryResponse(
                    r.Id.Value,
                    r.Author,
                    r.Rating,
                    r.Comment,
                    r.CreatedAtUtc)
            })
            .ToListAsync(cancellationToken);

        var lookup = reviewsByProduct
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ReviewSummaryResponse>)g.Select(x => x.Review).ToList());

        return products
            .Select(product => new ProductCatalogItemResponse(
                product.Id.Value,
                product.Name,
                product.Price,
                lookup.TryGetValue(product.Id, out var reviews) ? reviews : Array.Empty<ReviewSummaryResponse>()))
            .ToList();
    }
}
