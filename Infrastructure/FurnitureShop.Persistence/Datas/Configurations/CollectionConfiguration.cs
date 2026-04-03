using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ImagesUrl).HasMaxLength(500);

        builder.HasOne(x => x.CollectionCategory)
            .WithMany(c => c.Collections)
            .HasForeignKey(x => x.CollectionCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Products).WithMany();

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Collection)
            .HasForeignKey(t => t.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CollectionTranslationConfiguration : IEntityTypeConfiguration<CollectionTranslation>
{
    public void Configure(EntityTypeBuilder<CollectionTranslation> builder)
    {
        builder.HasIndex(x => new { x.CollectionId, x.Lang }).IsUnique();
        builder.Property(x => x.Lang).HasMaxLength(5);
        builder.Property(x => x.Name).HasMaxLength(200);
    }
}

public class CollectionCategoryConfiguration : IEntityTypeConfiguration<CollectionCategory>
{
    public void Configure(EntityTypeBuilder<CollectionCategory> builder)
    {
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ImageUrl).HasMaxLength(500);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.CollectionCategory)
            .HasForeignKey(t => t.CollectionCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CollectionCategoryTranslationConfiguration : IEntityTypeConfiguration<CollectionCategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CollectionCategoryTranslation> builder)
    {
        builder.HasIndex(x => new { x.CollectionCategoryId, x.Lang }).IsUnique();
        builder.Property(x => x.Lang).HasMaxLength(5);
        builder.Property(x => x.Name).HasMaxLength(100);
    }
}
