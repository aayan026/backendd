namespace FurnitureShop.Application.Dtos.Payment;

public class StripeIntentDto
{
    public string ClientSecret { get; set; } = null!;
    public string PaymentIntentId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "azn";
}