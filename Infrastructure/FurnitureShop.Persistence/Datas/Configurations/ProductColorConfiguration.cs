using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class ProductColorConfiguration : IEntityTypeConfiguration<ProductColor>
{
    public void Configure(EntityTypeBuilder<ProductColor> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.Property(x => x.HexCode).HasMaxLength(10);
        builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired(false);
    }
}