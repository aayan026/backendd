using FurnitureShop.Domain.Entities.Concretes.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.HasIndex(x => new { x.ProductId, x.Lang }).IsUnique();

        builder.Property(x => x.Lang).HasMaxLength(5);
        builder.Property(x => x.Name).HasMaxLength(200);
    }
}
