namespace FurnitureShop.Application.Services.Abstracts;

public interface IPaymentService
{
    /// <summary>
    /// Payriff vasitəsilə ödəniş başladır, ödəniş URL-i qaytarır
    /// </summary>
    Task<string> InitiateAsync(int orderId, decimal amount, string description, string lang);

    /// <summary>
    /// Payriff callback-dən gələn ödənişi yoxlayır
    /// </summary>
    Task<bool> VerifyAsync(string payriffOrderId, string sessionId);
}
