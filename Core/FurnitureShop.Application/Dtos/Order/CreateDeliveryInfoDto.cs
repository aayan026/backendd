using FurnitureShop.Domain.Entities.Enums;
namespace FurnitureShop.Application.Dtos.Order;
public class CreateDeliveryInfoDto
{
    public DeliveryType DeliveryType { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? TimeSlot { get; set; }
    public int Floor { get; set; } = 0;
    public bool HasElevator { get; set; }
    public bool RemoveOldFurniture { get; set; }
    public string? DeliveryNote { get; set; }
}
