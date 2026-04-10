using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.PriceExtra)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Material)
            .HasMaxLength(100);

        builder.Property(x => x.Label)
            .HasMaxLength(50);

        builder.Property(x => x.Width).HasColumnType("decimal(8,2)");
        builder.Property(x => x.Height).HasColumnType("decimal(8,2)");
        builder.Property(x => x.Depth).HasColumnType("decimal(8,2)");
        builder.Property(x => x.Weight).HasColumnType("decimal(8,2)");

        // Optimistic concurrency token — [Timestamp] atributu EF üçün kifayətdir,
        // lakin explicit konfiqurasiya daha aydındır
        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(x => x.FurnitureCategory)
            .WithMany(c => c.Products)
            .HasForeignKey(x => x.FurnitureCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Colors)
            .WithOne(c => c.Product)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Product)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
