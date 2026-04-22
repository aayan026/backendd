using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Application.Dtos.Order;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
    public string? AdminNote { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}
