using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Application.Dtos.Order;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
