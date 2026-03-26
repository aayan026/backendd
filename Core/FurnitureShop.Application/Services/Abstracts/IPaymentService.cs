using FurnitureShop.Application.Dtos.Payment;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IPaymentService
{
    Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(CreatePaymentIntentDto dto);
    Task<bool> ConfirmPaymentAsync(ConfirmPaymentDto dto);
    Task HandleWebhookAsync(string payload, string signature);
}
