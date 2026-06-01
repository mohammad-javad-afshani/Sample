namespace Domain.Products;

public interface IProductRepository
{
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
    Task<Product?> FindByIdAsync(ProductId id);
    Task<List<Product>> FindByCategoryAsync(string category);
    Task<List<Product>> GetAllAsync();
}
