using FurnitureShop.Domain.Entities.Enums;
namespace FurnitureShop.Application.Dtos.Order;
public class OrderDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Note { get; set; }
    public string? DiscountCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DeliveryInfoDto? DeliveryInfo { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
