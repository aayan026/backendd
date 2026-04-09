namespace FurnitureShop.Application.Dtos.Payment;

public class VerifyPaymentDto
{
    public int    OrderId       { get; set; }
    public string PayriffOrderId { get; set; } = default!;
    public string SessionId     { get; set; } = default!;
}
