using FurnitureShop.Application.Services.Abstracts;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FurnitureShop.Infrastructure.Services.Concretes;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration  _config;
    private readonly IOrderService   _orderService;
    private readonly HttpClient      _http;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    };

    public PaymentService(IConfiguration config, IOrderService orderService, IHttpClientFactory httpFactory)
    {
        _config       = config;
        _orderService = orderService;
        _http         = httpFactory.CreateClient("Payriff");
    }

    // ── Ödənişi Başlat ───────────────────────────────────────────────────
    public async Task<string> InitiateAsync(int orderId, decimal amount, string description, string lang)
    {
        var secret      = _config["Payriff:SecretKey"]!;
        var frontendUrl = _config["App:FrontendUrl"] ?? "http://localhost:5173";

        var langCode = lang switch { "ru" => "RU", "en" => "EN", _ => "AZ" };

        var body = new
        {
            amount,
            currencyType = "AZN",
            description,
            approveUrl = $"{frontendUrl}/payment/success?orderId={orderId}",
            cancelUrl  = $"{frontendUrl}/payment/cancel?orderId={orderId}",
            declineUrl = $"{frontendUrl}/payment/failed?orderId={orderId}",
            language   = langCode,
            cardSave   = false,
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.payriff.com/api/v2/createOrder")
        {
            Content = JsonContent.Create(body, options: _json),
        };
        request.Headers.TryAddWithoutValidation("Authorization", secret);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PayriffCreateResponse>(_json);

        if (result?.Code != "00000" || result.Payload?.PaymentUrl is null)
            throw new InvalidOperationException(
                $"Payriff xəta: {result?.Message ?? "Bilinmir"}");

        return result.Payload.PaymentUrl;
    }

    // ── Ödənişi Yoxla ────────────────────────────────────────────────────
    public async Task<bool> VerifyAsync(string payriffOrderId, string sessionId)
    {
        var secret = _config["Payriff:SecretKey"]!;

        var body = new { orderId = payriffOrderId, sessionId };

        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.payriff.com/api/v2/getOrderStatus")
        {
            Content = JsonContent.Create(body, options: _json),
        };
        request.Headers.TryAddWithoutValidation("Authorization", secret);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<PayriffStatusResponse>(_json);

        if (result?.Code != "00000") return false;

        var approved = string.Equals(
            result.Payload?.Status, "APPROVED", StringComparison.OrdinalIgnoreCase);

        return approved;
    }

    // ── Internal DTOs ────────────────────────────────────────────────────
    private sealed class PayriffCreateResponse
    {
        [JsonPropertyName("code")]    public string? Code    { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("payload")] public PayriffCreatePayload? Payload { get; set; }
    }

    private sealed class PayriffCreatePayload
    {
        [JsonPropertyName("orderId")]    public string? OrderId    { get; set; }
        [JsonPropertyName("sessionId")]  public string? SessionId  { get; set; }
        [JsonPropertyName("paymentUrl")] public string? PaymentUrl { get; set; }
    }

    private sealed class PayriffStatusResponse
    {
        [JsonPropertyName("code")]    public string? Code    { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("payload")] public PayriffStatusPayload? Payload { get; set; }
    }

    private sealed class PayriffStatusPayload
    {
        [JsonPropertyName("status")] public string? Status { get; set; }
    }
}
