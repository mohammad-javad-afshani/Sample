namespace Domain.Products;

public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(ProductId productId, int requested, int available)
        : base($"Product {productId.Value} has insufficient stock. Requested {requested}, available {available}.")
    {
    }
}
