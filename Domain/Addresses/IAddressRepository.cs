using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Addresses
{
    public interface IAddressRepository
    {
        void Add(Address address);   
    }
}
