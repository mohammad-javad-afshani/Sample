using Domain.Orders;
using Domain.Refunds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasConversion(
            id => id.Value,
            value => new RefundId(value));

        builder.Property(r => r.OrderId).HasConversion(
            id => id.Value,
            value => new OrderId(value));

        builder.Property(r => r.Amount).HasPrecision(18, 2);
        builder.Property(r => r.Reason).HasMaxLength(500);
        builder.Property(r => r.Status).HasConversion<int>();
        builder.HasIndex(r => r.OrderId);
    }
}
