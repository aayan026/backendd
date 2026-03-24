using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureShop.Persistence.Datas.Configurations;

public class DeliveryInfoConfiguration : IEntityTypeConfiguration<DeliveryInfo>
{
    public void Configure(EntityTypeBuilder<DeliveryInfo> builder)
    {
        builder.Property(x => x.DeliveryCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TimeSlot).HasMaxLength(20);
        builder.Property(x => x.DeliveryNote).HasMaxLength(500);
    }
}
