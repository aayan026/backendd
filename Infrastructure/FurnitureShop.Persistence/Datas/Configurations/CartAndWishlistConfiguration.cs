using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.Property(x => x.SelectedColor).HasMaxLength(50);
        builder.Property(x => x.SelectedSize).HasMaxLength(50);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Collection)
            .WithMany()
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Wishlist)
            .HasForeignKey(i => i.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Collection)
            .WithMany()
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
