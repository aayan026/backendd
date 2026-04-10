using FurnitureShop.Domain.Entities.Enums;
namespace FurnitureShop.Application.Dtos.Order;
public class DeliveryInfoDto
{
    public DeliveryType DeliveryType { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? TimeSlot { get; set; }
    public int Floor { get; set; }
    public bool HasElevator { get; set; }
    public bool RemoveOldFurniture { get; set; }
    public string? DeliveryNote { get; set; }
    public decimal DeliveryCost { get; set; }
    public DeliveryStatus Status { get; set; }
}
