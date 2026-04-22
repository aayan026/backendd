using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Domain.Entities.Concretes;

public class DeliveryInfo : BaseEntity
{
    public int OrderId { get; set; }

    public DeliveryType DeliveryType { get; set; }

    public DateTime? ScheduledDate { get; set; }
    public string? TimeSlot { get; set; }

    public int Floor { get; set; } = 0;
    public bool HasElevator { get; set; } = false;
    public bool RemoveOldFurniture { get; set; } = false;

    public string? DeliveryNote { get; set; }
    public decimal DeliveryCost { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.Scheduled;

    public Order Order { get; set; } = null!;
}
