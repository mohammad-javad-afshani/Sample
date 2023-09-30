using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customers
{
    public class CustomerNotValidExeption : Exception
    {
        public CustomerNotValidExeption()
            : base($"The Customer is not valid.")
        {
        }
    }
}
