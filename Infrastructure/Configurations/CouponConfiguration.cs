using Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasConversion(
            id => id.Value,
            value => new CouponId(value));

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.PercentOff).HasPrecision(5, 2);
        builder.HasIndex(c => c.Code).IsUnique();
    }
}
