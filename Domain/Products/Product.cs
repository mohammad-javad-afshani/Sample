namespace Domain.Products;

public class Product
{
    public ProductId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal InternalCost { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public int StockQuantity { get; internal set; }
    public int ViewCount { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private Product() { }

    public Product(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ProductNotValidExeption("Product name is required.");
        }

        if (price < 0)
        {
            throw new ProductNotValidExeption("Product price cannot be negative.");
        }

        Id = new ProductId(Guid.NewGuid());
        Name = name.Trim();
        Price = price;
    }

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
        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        InternalCost = internalCost;
        Category = category.Trim();
        StockQuantity = stockQuantity;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        if (StockQuantity < quantity)
        {
            throw new InsufficientStockException(Id, quantity, StockQuantity);
        }

        StockQuantity -= quantity;
    }

    public void AdjustStock(int delta)
    {
        if (delta == 0)
        {
            return;
        }

        var newQuantity = StockQuantity + delta;
        if (newQuantity < 0)
        {
            throw new InsufficientStockException(Id, Math.Abs(delta), StockQuantity);
        }

        StockQuantity = newQuantity;
    }
}
