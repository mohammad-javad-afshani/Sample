using Application.Data;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    internal sealed class CustomerRepository : ICustomerRepository
    {
        private readonly IApplicationDbContext _context;
        public CustomerRepository(ApplicationDbContext context) 
        {
            _context = context; 
        }
        public void Add(Customer customer)
        {
            _context.Customers.Add(customer);
        }

        public void Delete(Customer customer)
        {
            _context.Customers.Remove(customer);
        }

        public async Task<Customer> FindByIdAsync(CustomerId id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        }

        public void Update(Customer customer)
        {
            _context.Customers.Update(customer);    
        }
    }
}
