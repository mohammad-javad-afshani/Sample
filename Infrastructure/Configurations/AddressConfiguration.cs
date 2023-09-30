using Domain.Addresses;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations
{
    internal class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.Property(a => a.CountryTitle).HasColumnName("countrytitle");
            builder.Property(a => a.CityTitle).HasColumnName("cityTitle");
            builder.Property(a => a.Id).HasColumnName("id");
            builder.Property(a => a.Street).HasColumnName("street");
            builder.Property(a => a.PostalCode).HasColumnName("postalCode");
        }
    }
}
