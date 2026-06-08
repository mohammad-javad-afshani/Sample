using Domain.Addresses;
using Domain.Customers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Customers.Update
{
    public record UpdateCustomerCommand(
        CustomerId customerId,
        string Firstname,
        string Lastname,
        DateTime DateOfBirth,
        string Email,
        string PhoneNumber,
        string BankAccountNumber,
        List<Address> Addresses) : IRequest;

    public record UpdateCustomerRequest(
     
      string Firstname,
      string Lastname,
      DateTime DateOfBirth,
      string Email,
      string PhoneNumber,
      string BankAccountNumber,
      List<Address> Addresses);

}
