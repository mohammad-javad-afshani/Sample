using Application.Data;
using Domain.Addresses;
using Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    internal sealed class AddressRepository : IAddressRepository
    {
        private readonly IApplicationDbContext _context;
        public AddressRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Add(Address address)
        {
            _context.Addresses.Add(address);
        }
    }
}
