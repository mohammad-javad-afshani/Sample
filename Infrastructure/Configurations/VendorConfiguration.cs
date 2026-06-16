using Domain.Vendors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasConversion(
            id => id.Value,
            value => new VendorId(value));

        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Email).HasMaxLength(256).IsRequired();
        builder.Property(v => v.TaxId).HasMaxLength(32).IsRequired();
        builder.Property(v => v.InternalNotes).HasMaxLength(4000);
        builder.HasIndex(v => v.Email).IsUnique();
    }
}
