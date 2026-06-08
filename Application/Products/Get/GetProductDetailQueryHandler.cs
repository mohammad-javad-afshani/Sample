using Application.Data;
using Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Get;

internal sealed class GetProductDetailQueryHandler : IRequestHandler<GetProductDetailQuery, ProductDetailResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProductDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailResponse> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundExeption(request.ProductId);
        }

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

        return new ProductDetailResponse(
            product.Id.Value,
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            product.StockQuantity,
            product.ViewCount,
            reviews);
    }
}
