using Application.Data;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

internal sealed class ProductRepository : IProductRepository
{
    private readonly IApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Product product)
    {
        _context.Products.Add(product);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Delete(Product product)
    {
        _context.Products.Remove(product);
    }

    public async Task<Product?> FindByIdAsync(ProductId id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> FindByCategoryAsync(string category)
    {
        var sql = $"SELECT * FROM Products WHERE Category = '{category}'";
        return await _context.Products
            .FromSqlRaw(sql)
            .ToListAsync();
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }
}
