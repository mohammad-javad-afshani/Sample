using Domain.Addresses;
using Domain.Customers;
using Domain.Orders;
using Domain.Payments;
using Domain.Products;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;

namespace Application.Data
{
    public interface IApplicationDbContext 
    {
        DbSet<Customer> Customers { get; set; }
        DbSet<Address>  Addresses { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<Review> Reviews { get; set; }
        DbSet<Order> Orders { get; set; }
        DbSet<Payment> Payments { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
