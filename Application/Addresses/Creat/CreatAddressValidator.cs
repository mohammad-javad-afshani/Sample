using Domain.Addresses;
using Domain.Customers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Addresses.Creat
{
    public class CreatAddressValidator : AbstractValidator<Address>
    {
        public CreatAddressValidator() 
        {
            RuleFor(address => address.PostalCode)
                .NotEmpty().WithMessage("PostalCode is required.")
                .Length(10).WithMessage("PostalCode should be 10 characters.")
                .Matches("^[0-9]+$").WithMessage("PostalCode must contain only numeric digits.");
        }  
    }
}
