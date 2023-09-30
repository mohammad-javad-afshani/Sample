using Domain.Customers;
using FluentValidation;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Customers.Creat
{
    public class CreatCustomerValidator : AbstractValidator<Customer>
    {
        public CreatCustomerValidator()
        {
            RuleFor(customer => customer.Email)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(255).WithMessage("Name should be less than 255 characters.");
            RuleFor(customer => customer.BankAccountNumber)
                .NotEmpty().WithMessage("BankAccountNumber is required.")
                .Length(24).WithMessage("BankAccountNumber should be 24 characters.")
                .Matches("^[0-9]+$").WithMessage("BankAccountNumber must contain only numeric digits.");
            RuleFor(customer => customer.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required.")
                .MaximumLength(15).WithMessage("PhoneNumber should be less than 16 digits")
                .Must(BeAValidPhoneNumber).WithMessage("Invalid phone number.");
        }
        private bool BeAValidPhoneNumber(string phoneNumber)
        {
            PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
            try
            {
                var phoneNumberProto = phoneNumberUtil.Parse(phoneNumber, null);
                return phoneNumberUtil.IsValidNumber(phoneNumberProto);
            }
            catch (NumberParseException)
            {
                return false;
            }
        }
    }
}
