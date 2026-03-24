using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class FurnitureCategoryConfiguration : IEntityTypeConfiguration<FurnitureCategory>
{
    public void Configure(EntityTypeBuilder<FurnitureCategory> builder)
    {
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.FurnitureCategory)
            .HasForeignKey(t => t.FurnitureCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FurnitureCategoryTranslationConfiguration : IEntityTypeConfiguration<FurnitureCategoryTranslation>
{
    public void Configure(EntityTypeBuilder<FurnitureCategoryTranslation> builder)
    {
        builder.HasIndex(x => new { x.FurnitureCategoryId, x.Lang }).IsUnique();
        builder.Property(x => x.Lang).HasMaxLength(5);
        builder.Property(x => x.Name).HasMaxLength(100);
    }
}
