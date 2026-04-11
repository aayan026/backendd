using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ShippingCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.Property(x => x.AdminNote).HasMaxLength(1000);
        builder.Property(x => x.CustomDescription)
.IsRequired(false);
        builder.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MonthlyPayment).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Address)
            .WithMany(a => a.Orders)
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DiscountCode)
            .WithMany(d => d.Orders)
            .HasForeignKey(x => x.DiscountCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.DeliveryInfo)
            .WithOne(d => d.Order)
            .HasForeignKey<DeliveryInfo>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
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

        builder.Property(x => x.CustomDescription)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(x => x.SelectedColor)
    .HasMaxLength(50)
    .IsRequired(false);

        builder.Property(x => x.SelectedSize)
            .HasMaxLength(50)
            .IsRequired(false);
    }
}
