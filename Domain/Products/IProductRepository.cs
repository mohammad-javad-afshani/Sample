namespace Domain.Products;

public interface IProductRepository
{
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
    Task<Product?> FindByIdAsync(ProductId id, CancellationToken cancellationToken = default);
    Task<(List<Product> Items, int TotalCount)> FindPagedAsync(
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
