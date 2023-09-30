using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customers
{
    public class CustomerNotFoundExeption : Exception
    {
        public CustomerNotFoundExeption(CustomerId id)
            :base($"The Customer With the ID = {id.Value} was not found") 
        {
        }
    }
}
