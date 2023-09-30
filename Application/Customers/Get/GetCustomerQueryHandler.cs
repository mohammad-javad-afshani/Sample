using Application.Data;
using Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Customers.Get
{
    internal sealed class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerResponse>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerResponse> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers.
                Where(c => c.Id == request.customerId).
                Select(c => new CustomerResponse(
                    c.Id.Value,
                    c.Firstname,
                    c.Lastname,
                    c.DateOfBirth,
                    c.Email,
                    c.PhoneNumber,
                    c.BankAccountNumber,
                    c.Addresses
                    )).FirstOrDefaultAsync(cancellationToken);
            if (customer == null) 
            {
                throw new CustomerNotFoundExeption(request.customerId);
            }
            return customer;
        } 
    }
}
