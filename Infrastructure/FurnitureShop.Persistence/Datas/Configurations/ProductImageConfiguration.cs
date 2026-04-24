using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class ProductColorImageConfiguration : IEntityTypeConfiguration<ProductColorImage>
{
    public void Configure(EntityTypeBuilder<ProductColorImage> builder)
    {
        builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);

        builder.HasOne(x => x.ProductColor)
               .WithMany(x => x.ColorImages)
               .HasForeignKey(x => x.ProductColorId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}