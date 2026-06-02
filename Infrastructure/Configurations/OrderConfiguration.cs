using Domain.Customers;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasConversion(
            id => id.Value,
            value => new OrderId(value));

        builder.Property(o => o.CustomerId).HasConversion(
            id => id == null ? (Guid?)null : id.Value,
            value => value == null ? null : new CustomerId(value.Value));

        builder.Property(o => o.Status).HasConversion<int>();
        builder.OwnsMany(o => o.Lines, lines =>
        {
            lines.WithOwner().HasForeignKey("OrderId");
            lines.Property<Guid>("Id");
            lines.HasKey("Id");
            lines.Property(l => l.ProductId).HasConversion(
                id => id.Value,
                value => new Domain.Products.ProductId(value));
            lines.Property(l => l.UnitPrice).HasPrecision(18, 2);
        });

        builder.Navigation(o => o.Lines).AutoInclude();
    }
}
