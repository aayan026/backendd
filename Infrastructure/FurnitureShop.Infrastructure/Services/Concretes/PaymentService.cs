using FurnitureShop.Application.Dtos.Payment;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace FurnitureShop.Infrastructure.Services.Concretes;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _config;
    private readonly IOrderService _orderService;

    public PaymentService(IConfiguration config, IOrderService orderService)
    {
        _config = config;
        _orderService = orderService;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
    }

    public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(CreatePaymentIntentDto dto)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(dto.Amount * 100),
            Currency = dto.Currency.ToLower(),
            Metadata = new Dictionary<string, string>
            {
                { "orderId", dto.OrderId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        return new PaymentIntentResponseDto
        {
            ClientSecret = intent.ClientSecret,
            PaymentIntentId = intent.Id
        };
    }

    public async Task<bool> ConfirmPaymentAsync(ConfirmPaymentDto dto)
    {
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(dto.PaymentIntentId);

        if (intent.Status == "succeeded")
        {
            await _orderService.MarkPaymentPaidAsync(dto.OrderId);
            return true;
        }

        return false;
    }

    public async Task HandleWebhookAsync(string payload, string signature)
    {
        var webhookSecret = _config["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

        var stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                var intent = stripeEvent.Data.Object as PaymentIntent;
                if (intent?.Metadata.TryGetValue("orderId", out var orderIdStr) == true
                    && int.TryParse(orderIdStr, out var orderId))
                    await _orderService.MarkPaymentPaidAsync(orderId);
                break;

            case "payment_intent.payment_failed":
                var failed = stripeEvent.Data.Object as PaymentIntent;
                if (failed?.Metadata.TryGetValue("orderId", out var failedIdStr) == true
                    && int.TryParse(failedIdStr, out var failedId))
                    await _orderService.MarkPaymentFailedAsync(failedId);
                break;
        }
    }
}
