using FurnitureShop.Application.Dtos.Email;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IEmailService
{
    Task SendAsync(SendEmailDto dto);
    Task SendForgotPasswordAsync(string toEmail, string toName, string resetToken, string lang);
    Task SendOrderConfirmationAsync(string toEmail, string toName, int orderId, decimal total, string lang);
    Task SendOrderStatusChangedAsync(string toEmail, string toName, int orderId, string status, string lang);
}
