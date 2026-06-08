namespace Domain.Products;

public class ProductNotValidExeption : Exception
{
    public ProductNotValidExeption(string message) : base(message)
    {
    }
}
