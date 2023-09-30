using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Customers;
using Domain.Addresses;
using System.Reflection.Emit;

namespace Persistence.Configurations

{
    internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id).HasConversion(
                customerId => customerId.Value,
                value => new CustomerId(value));

            builder.Property(c => c.Firstname).HasColumnName("firstname");
            builder.Property(c => c.DateOfBirth).HasColumnName("dateofbirth");
            builder.Property(c => c.PhoneNumber).HasColumnName("phonenumber");
            builder.Property(c => c.BankAccountNumber).HasColumnName("bankaccountnumber");
            builder.Property(c => c.Email).HasColumnName("email");
            builder.Property(c => c.Lastname).HasColumnName("lastname");
            builder.Property(c => c.PhoneNumber).HasMaxLength(15);
            builder.Property(c => c.Email).HasMaxLength(255);
            builder.HasMany(c => c.Addresses);
       
        }
       
    }
}
