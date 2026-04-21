using FurnitureShop.Application.Dtos.Payment;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Enums;
using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Serilog;
using Stripe;

namespace FurnitureShop.Persistence.Services.Concretes;

public class PaymentService : IPaymentService
{
    private readonly IOrderReadRepository _readRepo;
    private readonly IOrderWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly UserManager<AppUser> _userManager;
    private readonly string _webhookSecret;

    private static readonly ILogger _log = Log.ForContext<PaymentService>();

    public PaymentService(
        IOrderReadRepository readRepo,
        IOrderWriteRepository writeRepo,
        ILanguageService langService,
        UserManager<AppUser> userManager,
        IConfiguration config)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _userManager = userManager;
        _webhookSecret = config["Stripe:WebhookSecret"] ?? "";

        StripeConfiguration.ApiKey = config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey konfiqurasiya edilməyib.");
    }

    private string Lang => _langService.GetCurrentLanguage();

    public async Task<StripeIntentDto> CreatePaymentIntentAsync(int orderId, string userId)
    {
        _log.Information("PaymentIntent yaradılır — OrderId: {OrderId} UserId: {UserId}", orderId, userId);

        var order = await _readRepo.GetWithDetailsAsync(orderId, Lang)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        if (order.UserId != userId)
            throw new ForbiddenException(ValidationMessages.Get(Lang, "OrderAccessForbidden"));

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> {
                    { "payment", new List<string> { "Bu sifariş artıq ödənilib." } }
                });

        var amountInCents = (long)(order.TotalPrice * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = "azn",
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = orderId.ToString(),
                ["userId"] = userId
            },
            SetupFutureUsage = null,
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        _log.Information("PaymentIntent yaradıldı — IntentId: {IntentId} Amount: {Amount}",
            intent.Id, order.TotalPrice);

        return new StripeIntentDto
        {
            ClientSecret = intent.ClientSecret,
            PaymentIntentId = intent.Id,
            Amount = order.TotalPrice,
            Currency = "azn"
        };
    }

    public async Task HandleWebhookAsync(string payload, string stripeSignature)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, stripeSignature, _webhookSecret);
        }
        catch (StripeException ex)
        {
            _log.Warning("Webhook imza xətası: {Message}", ex.Message);
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> {
                    { "webhook", new List<string> { "Webhook imzası yanlışdır." } }
                });
        }

        _log.Information("Stripe webhook alındı — Type: {Type}", stripeEvent.Type);

        if (stripeEvent.Data.Object is not PaymentIntent intent)
        {
            _log.Warning("Webhook: PaymentIntent deyil, keçilir — Type: {Type}", stripeEvent.Type);
            return;
        }

        if (!intent.Metadata.TryGetValue("orderId", out var orderIdStr)
            || !int.TryParse(orderIdStr, out var orderId))
        {
            _log.Warning("Webhook: orderId metadata tapılmadı — IntentId: {Id}", intent.Id);
            return;
        }

        switch (stripeEvent.Type)
        {
            case Stripe.Events.PaymentIntentSucceeded:
                await MarkPaidAsync(orderId, intent.Id);
                break;

            case Stripe.Events.PaymentIntentPaymentFailed:
                await MarkFailedAsync(orderId, intent.Id);
                break;

            default:
                _log.Information("Webhook: emal edilməyən event — {Type}", stripeEvent.Type);
                break;
        }
    }

    private async Task MarkPaidAsync(int orderId, string intentId)
    {
        var order = await _readRepo.GetByIdAsync(orderId);
        if (order is null) { _log.Warning("MarkPaid: sifariş tapılmadı — {OrderId}", orderId); return; }

        if (order.PaymentStatus == PaymentStatus.Paid)
        { _log.Information("MarkPaid: artıq ödənilib — {OrderId}", orderId); return; }

        order.PaymentStatus = PaymentStatus.Paid;
        order.PaidAmount = order.TotalPrice;
        if (order.Status == OrderStatus.Pending) order.Status = OrderStatus.Confirmed;

        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Ödəniş uğurlu — OrderId: {OrderId} IntentId: {IntentId}", orderId, intentId);
    }

    private async Task MarkFailedAsync(int orderId, string intentId)
    {
        var order = await _readRepo.GetByIdAsync(orderId);
        if (order is null) return;
        if (order.PaymentStatus == PaymentStatus.Paid) return;

        order.PaymentStatus = PaymentStatus.Failed;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();
        _log.Warning("Ödəniş uğursuz — OrderId: {OrderId} IntentId: {IntentId}", orderId, intentId);
    }
}