using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)");
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.ButtonLink).HasMaxLength(300);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Campaign)
            .HasForeignKey(t => t.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CampaignTranslationConfiguration : IEntityTypeConfiguration<CampaignTranslation>
{
    public void Configure(EntityTypeBuilder<CampaignTranslation> builder)
    {
        builder.HasIndex(x => new { x.CampaignId, x.Lang }).IsUnique();
        builder.Property(x => x.Lang).HasMaxLength(5);
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.ButtonText).HasMaxLength(50);
    }
}

public class HeroSectionConfiguration : IEntityTypeConfiguration<HeroSection>
{
    public void Configure(EntityTypeBuilder<HeroSection> builder)
    {
        builder.Property(x => x.ImageUrl).HasMaxLength(500);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.HeroSection)
            .HasForeignKey(t => t.HeroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HeroTranslationConfiguration : IEntityTypeConfiguration<HeroTranslation>
{
    public void Configure(EntityTypeBuilder<HeroTranslation> builder)
    {
        builder.HasIndex(x => new { x.HeroId, x.Lang }).IsUnique();
        builder.Property(x => x.Lang).HasMaxLength(5);
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.Subtitle).HasMaxLength(300);
        builder.Property(x => x.BadgeText).HasMaxLength(50);
    }
}

public class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> builder)
    {
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.Value).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MinOrderAmount).HasColumnType("decimal(18,2)");
    }
}
