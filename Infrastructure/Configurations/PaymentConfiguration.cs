using Domain.Orders;
using Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasConversion(
            id => id.Value,
            value => new PaymentId(value));

        builder.Property(p => p.OrderId).HasConversion(
            id => id.Value,
            value => new OrderId(value));

        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Status).HasConversion<int>();
        builder.HasIndex(p => p.OrderId);
    }
}
