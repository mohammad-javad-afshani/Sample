using Domain.Addresses;
using Domain.Customers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Customers.Creat
{
    public  record CreatCustomerCommand(
        string Firstname,
        string Lastname,
        DateTime DateOfBirth,
        string Email,
        string PhoneNumber,
        string BankAccountNumber,
        List<Address> Addresses) : IRequest;
    

    
}
