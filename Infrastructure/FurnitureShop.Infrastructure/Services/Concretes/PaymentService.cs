using FurnitureShop.Application.Dtos.Payment;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FurnitureShop.Infrastructure.Services.Concretes;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentService> _logger;
    private readonly IOrderService _orderService;

    public PaymentService(IConfiguration config, ILogger<PaymentService> logger, IOrderService orderService)
    {
        _config = config;
        _logger = logger;
        _orderService = orderService;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
    }

    public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(CreatePaymentIntentDto dto)
    {
        var amountInCents = (long)(dto.Amount * 100);
        var options = new PaymentIntentCreateOptions
        {
            Amount   = amountInCents,
            Currency = dto.Currency.ToLower(),
            Metadata = new Dictionary<string, string>
            {
                { "orderId", dto.OrderId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options);

        _logger.LogInformation("PaymentIntent created: {IntentId} for Order: {OrderId}", intent.Id, dto.OrderId);

        return new PaymentIntentResponseDto
        {
            ClientSecret    = intent.ClientSecret,
            PaymentIntentId = intent.Id
        };
    }

    public async Task<bool> ConfirmPaymentAsync(ConfirmPaymentDto dto)
    {
        var service = new PaymentIntentService();
        var intent  = await service.GetAsync(dto.PaymentIntentId);

        if (intent.Status == "succeeded")
        {
            await _orderService.MarkPaymentPaidAsync(dto.OrderId);
            _logger.LogInformation("Payment confirmed for Order: {OrderId}", dto.OrderId);
            return true;
        }

        _logger.LogWarning("Payment not succeeded for Order: {OrderId}. Status: {Status}", dto.OrderId, intent.Status);
        return false;
    }

    public async Task HandleWebhookAsync(string payload, string signature)
    {
        var webhookSecret = _config["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            throw;
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent?.Metadata.TryGetValue("orderId", out var orderIdStr) == true
                    && int.TryParse(orderIdStr, out var orderId))
                {
                    await _orderService.MarkPaymentPaidAsync(orderId);
                    _logger.LogInformation("Webhook: payment succeeded for Order {OrderId}", orderId);
                }
                break;

            case "payment_intent.payment_failed":
                var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                if (failedIntent?.Metadata.TryGetValue("orderId", out var failedOrderIdStr) == true
                    && int.TryParse(failedOrderIdStr, out var failedOrderId))
                {
                    await _orderService.MarkPaymentFailedAsync(failedOrderId);
                    _logger.LogWarning("Webhook: payment failed for Order {OrderId}", failedOrderId);
                }
                break;

            default:
                _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }
    }
}
