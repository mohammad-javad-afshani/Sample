using Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Addresses
{
    public class Address
    {
        public Address() { }
        public Address(string countrytitle, string citytitle, string postalcode, string street) 
        {
            Id = Guid.NewGuid();
            CountryTitle = countrytitle;
            CityTitle = citytitle;  
            PostalCode = postalcode;    
            Street = street;
        }    
        public Guid Id { get; set; }
        public string CountryTitle { get; private set; }
        public string CityTitle { get; private set; }
        public string PostalCode { get; private set;}
        public string Street { get; private set;}   

       
    }
}
