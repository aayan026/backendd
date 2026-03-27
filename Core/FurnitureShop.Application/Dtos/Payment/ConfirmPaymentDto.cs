namespace FurnitureShop.Application.Dtos.Payment;

public class ConfirmPaymentDto
{
    public string PaymentIntentId { get; set; } = null!;
    public int OrderId { get; set; }
}
