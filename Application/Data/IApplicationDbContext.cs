using Domain.Addresses;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Application.Data
{
    public interface IApplicationDbContext 
    {
        DbSet<Customer> Customers { get; set; }
        DbSet<Address>  Addresses { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
