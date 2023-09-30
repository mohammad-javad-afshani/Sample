using Domain.Addresses;
using Domain.Customers;
using Microsoft.EntityFrameworkCore; 

namespace Application.Data
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
