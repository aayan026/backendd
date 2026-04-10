using FurnitureShop.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Surname)
            .IsRequired()
            .HasMaxLength(100);

        // SHA-256 hex = 64 char — köhnə 500-dən daha kiçik, migration tələb etmir
        builder.Property(x => x.RefreshToken)
            .HasMaxLength(500);
    }
}
