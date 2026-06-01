using Application.Products.Get;
using Domain.Products;

namespace Application.Products;

internal static class ProductResponseMapper
{
    public static ProductResponse ToResponse(Product product) =>
        new(
            product.Id.Value,
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            product.StockQuantity,
            product.ViewCount);
}
