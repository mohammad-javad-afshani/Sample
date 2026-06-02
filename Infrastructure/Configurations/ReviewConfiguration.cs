using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasConversion(
            id => id.Value,
            value => new ReviewId(value));

        builder.Property(r => r.ProductId).HasConversion(
            id => id.Value,
            value => new Domain.Products.ProductId(value));

        builder.Property(r => r.Author).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.HasIndex(r => r.ProductId);
    }
}
