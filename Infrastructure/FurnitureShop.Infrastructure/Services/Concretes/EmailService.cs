using FurnitureShop.Application.Dtos.Email;
using FurnitureShop.Application.Services.Abstracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace FurnitureShop.Infrastructure.Services.Concretes;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(SendEmailDto dto)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:SenderName"] ?? "FurnitureShop",
                _config["Email:SenderEmail"] ?? "noreply@furnitureshop.az"));
            message.To.Add(new MailboxAddress(dto.ToName, dto.ToEmail));
            message.Subject = dto.Subject;

            var builder = new BodyBuilder();
            if (dto.IsHtml)
                builder.HtmlBody = dto.Body;
            else
                builder.TextBody = dto.Body;

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Email:SmtpHost"] ?? "smtp.gmail.com",
                int.Parse(_config["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email} — Subject: {Subject}", dto.ToEmail, dto.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email göndərilə bilmədi: {Email}", dto.ToEmail);
        }
    }

    public async Task SendForgotPasswordAsync(string toEmail, string toName, string resetToken, string lang)
    {
        var frontendUrl = _config["App:FrontendUrl"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";

        var (subject, body) = lang switch
        {
            "ru" => ("Сброс пароля — FurnitureShop",
                     $"<h2>Здравствуйте, {toName}!</h2><p>Нажмите на кнопку ниже для сброса пароля:</p><p><a href='{resetLink}' style='background:#222;color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;'>Сбросить пароль</a></p><p>Ссылка действительна 1 час.</p><p><small>Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо.</small></p>"),
            "en" => ("Password Reset — FurnitureShop",
                     $"<h2>Hello, {toName}!</h2><p>Click the button below to reset your password:</p><p><a href='{resetLink}' style='background:#222;color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;'>Reset Password</a></p><p>The link is valid for 1 hour.</p><p><small>If you didn't request a password reset, please ignore this email.</small></p>"),
            _    => ("Şifrə yeniləmə — FurnitureShop",
                     $"<h2>Salam, {toName}!</h2><p>Şifrənizi yeniləmək üçün aşağıdakı düyməni klikləyin:</p><p><a href='{resetLink}' style='background:#222;color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;'>Şifrəni Yenilə</a></p><p>Keçid 1 saat etibarlıdır.</p><p><small>Əgər siz bu sorğunu göndərməmisinizsə, bu emaili nəzərə almayın.</small></p>")
        };

        await SendAsync(new SendEmailDto
        {
            ToEmail = toEmail,
            ToName  = toName,
            Subject = subject,
            Body    = body
        });
    }

    public async Task SendOrderConfirmationAsync(string toEmail, string toName, int orderId, decimal total, string lang)
    {
        var (subject, body) = lang switch
        {
            "ru" => ($"Заказ #{orderId} подтверждён — FurnitureShop",
                     $"<h2>Здравствуйте, {toName}!</h2><p>Ваш заказ <strong>#{orderId}</strong> успешно оформлен.</p><p>Итоговая сумма: <strong>{total:F2} AZN</strong></p><p>Мы уведомим вас о статусе заказа.</p>"),
            "en" => ($"Order #{orderId} Confirmed — FurnitureShop",
                     $"<h2>Hello, {toName}!</h2><p>Your order <strong>#{orderId}</strong> has been placed successfully.</p><p>Total: <strong>{total:F2} AZN</strong></p><p>We will notify you about your order status.</p>"),
            _    => ($"#{orderId} nömrəli sifarişiniz qəbul edildi — FurnitureShop",
                     $"<h2>Salam, {toName}!</h2><p><strong>#{orderId}</strong> nömrəli sifarişiniz uğurla qəbul edildi.</p><p>Ümumi məbləğ: <strong>{total:F2} AZN</strong></p><p>Sifariş statusu barədə sizi məlumatlandıracağıq.</p>")
        };

        await SendAsync(new SendEmailDto
        {
            ToEmail = toEmail,
            ToName  = toName,
            Subject = subject,
            Body    = body
        });
    }

    public async Task SendOrderStatusChangedAsync(string toEmail, string toName, int orderId, string status, string lang)
    {
        var (subject, body) = lang switch
        {
            "ru" => ($"Статус заказа #{orderId} изменён — FurnitureShop",
                     $"<h2>Здравствуйте, {toName}!</h2><p>Статус вашего заказа <strong>#{orderId}</strong> изменён на: <strong>{status}</strong></p>"),
            "en" => ($"Order #{orderId} Status Updated — FurnitureShop",
                     $"<h2>Hello, {toName}!</h2><p>Your order <strong>#{orderId}</strong> status has been updated to: <strong>{status}</strong></p>"),
            _    => ($"#{orderId} sifarişin statusu dəyişdi — FurnitureShop",
                     $"<h2>Salam, {toName}!</h2><p><strong>#{orderId}</strong> nömrəli sifarişinizin statusu <strong>{status}</strong> olaraq dəyişdirildi.</p>")
        };

        await SendAsync(new SendEmailDto
        {
            ToEmail = toEmail,
            ToName  = toName,
            Subject = subject,
            Body    = body
        });
    }
}
