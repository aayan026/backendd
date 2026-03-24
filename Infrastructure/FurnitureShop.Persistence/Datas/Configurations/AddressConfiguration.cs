using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ContactName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.District)
            .HasMaxLength(100);

        builder.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.ZipCode)
            .HasMaxLength(20);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
