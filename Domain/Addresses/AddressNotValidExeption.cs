using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Addresses
{
    public class AddressNotValidExeption : Exception
    {
        public AddressNotValidExeption()
            : base($"The Address is not valid.")
        {
        }
    }
}
