using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customers
{
    public interface ICustomerRepository
    {
        void Add(Customer customer);

        void Update(Customer customer);
        void Delete(Customer customer);
        
        public Task<Customer> FindByIdAsync(CustomerId id);
    }
}
