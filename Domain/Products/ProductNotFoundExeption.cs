namespace Domain.Products;

public class ProductNotFoundExeption : Exception
{
    public ProductNotFoundExeption(ProductId id)
        : base($"Product with id {id.Value} was not found.")
    {
    }
}
