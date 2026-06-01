namespace Domain.Products;

public class Product
{
    public ProductId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal InternalCost { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public int StockQuantity { get; private set; }
    public int ViewCount { get; private set; }

    private Product() { }

    public Product(string name, string description, decimal price, decimal internalCost, string category, int stockQuantity)
    {
        Id = new ProductId(Guid.NewGuid());
        Name = name;
        Description = description;
        Price = price;
        InternalCost = internalCost;
        Category = category;
        StockQuantity = stockQuantity;
        ViewCount = 0;
    }

    public void Update(string name, string description, decimal price, decimal internalCost, string category, int stockQuantity)
    {
        Name = name;
        Description = description;
        Price = price;
        InternalCost = internalCost;
        Category = category;
        StockQuantity = stockQuantity;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }
}
