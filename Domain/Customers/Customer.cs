using Domain.Addresses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customers
{
    public class Customer
    {
        public Customer() { }
        public Customer(string firstname, string lastname, DateTime dateofbirth, string email, string phonenumber, string bankaccount, List<Address> addresses) 
        {
            Id = new CustomerId( System.Guid.NewGuid());    
            Firstname = firstname;
            Lastname = lastname;
            DateOfBirth = dateofbirth;
            Email = email;
            PhoneNumber = phonenumber;
            BankAccountNumber = bankaccount;
            Addresses = addresses;  
        }
        public CustomerId Id { get; set; }
        public string Firstname { get; private set; }    
        public string Lastname { get; private set; } 
        public DateTime DateOfBirth { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public string BankAccountNumber { get; private set; }
        public List<Address> Addresses { get; private set; }
        
        public void Update(String firstname, string lastname, DateTime dateofbirth, string email, string phoneNumber, string bankaccount, List<Address> addresses) 
        {
            Firstname = firstname;
            Lastname = lastname;
            DateOfBirth = dateofbirth;
            Email = email;
            PhoneNumber = phoneNumber;  
            BankAccountNumber = bankaccount;    
            Addresses = addresses;
        }
    }
}
