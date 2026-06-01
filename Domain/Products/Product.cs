namespace Domain.Products;

public class Product
{
    public ProductId Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal InternalCost { get; set; }
    public string Category { get; set; }
    public int StockQuantity { get; set; }
    public int ViewCount { get; set; }

    public Product() { }

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
