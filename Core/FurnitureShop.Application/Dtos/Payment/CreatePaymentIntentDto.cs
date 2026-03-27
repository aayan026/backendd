namespace FurnitureShop.Application.Dtos.Payment;

public class CreatePaymentIntentDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "azn";
}
