using Application.Data;
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

        var catalog = new List<ProductCatalogItemResponse>();

        foreach (var product in products)
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == product.Id)
                .Select(r => new ReviewSummaryResponse(
                    r.Id.Value,
                    r.Author,
                    r.Rating,
                    r.Comment,
                    r.CreatedAtUtc))
                .ToListAsync(cancellationToken);

            catalog.Add(new ProductCatalogItemResponse(
                product.Id.Value,
                product.Name,
                product.Price,
                reviews));
        }

        return catalog;
    }
}
