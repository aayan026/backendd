using FurnitureShop.Application.Dtos.Payment;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IPaymentService
{
    Task<StripeIntentDto> CreatePaymentIntentAsync(int orderId, string userId);
    Task HandleWebhookAsync(string payload, string stripeSignature);
}