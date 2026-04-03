using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.AuthorName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.AuthorEmail)
            .HasMaxLength(200);

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
