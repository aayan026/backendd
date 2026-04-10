using FurnitureShop.Domain.Entities.Enums;
namespace FurnitureShop.Application.Dtos.Order;
public class CreateOrderDto
{
    public int? AddressId { get; set; }
    public int? DiscountCodeId { get; set; }
    public OrderType Type { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Note { get; set; }
    public bool IsCustomOrder { get; set; } = false;
    public string? CustomDescription { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? PaidAmount { get; set; }
    public int? InstallmentMonths { get; set; }
    public decimal? MonthlyPayment { get; set; }
    public CreateDeliveryInfoDto? DeliveryInfo { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
