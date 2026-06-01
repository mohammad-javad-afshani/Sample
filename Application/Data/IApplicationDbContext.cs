using Domain.Addresses;
using Domain.Customers;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Data
{
    public interface IApplicationDbContext 
    {
        DbSet<Customer> Customers { get; set; }
        DbSet<Address>  Addresses { get; set; }
        DbSet<Product> Products { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
