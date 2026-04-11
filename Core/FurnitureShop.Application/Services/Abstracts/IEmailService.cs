using FurnitureShop.Application.Dtos.Email;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IEmailService
{
    Task SendAsync(SendEmailDto dto);
    Task SendForgotPasswordAsync(string toEmail, string toName, string resetToken, string lang);
    Task SendOrderConfirmationAsync(string toEmail, string toName, int orderId, decimal total, string lang);
    Task SendOrderStatusChangedAsync(string toEmail, string toName, int orderId, string status,
        string? adminNote, DateTime? estimatedDelivery, string lang);
    Task SendAdminOrderNotificationAsync(
        int orderId, string customerName, string customerEmail, string customerPhone,
        decimal total, string paymentMethod, string deliveryNote,
        bool isCustomOrder, string? customDescription, string lang);

    /// <summary>Contact form — adminin emailinə bildiriş göndər</summary>
    Task SendContactNotificationAsync(
        string fromName, string fromEmail, string? fromPhone,
        string? subject, string message, string lang);
}
