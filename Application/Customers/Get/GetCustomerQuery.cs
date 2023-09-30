using Domain.Addresses;
using Domain.Customers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Customers.Get
{
    public record GetCustomerQuery(CustomerId customerId) : IRequest<CustomerResponse>;
    

    public record CustomerResponse(
        Guid Id,
        string Fistname,
        string Lastname,
        DateTime DateOfBirth,
        string Email,
        string PhoneNumber,
        string BankAccount,
        List<Address> Addresses
        );
}
