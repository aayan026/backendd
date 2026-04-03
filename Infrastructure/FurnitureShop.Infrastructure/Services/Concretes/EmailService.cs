using FurnitureShop.Application.Dtos.Email;
using FurnitureShop.Application.Services.Abstracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace FurnitureShop.Infrastructure.Services.Concretes;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    // ── Əsas göndərmə metodu ─────────────────────────────────────────────
    public async Task SendAsync(SendEmailDto dto)
    {
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];

        // E-poçt konfiqurasiya edilməyibsə log yaz, atma
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine($"[EmailService] SKIP (no credentials) → {dto.ToEmail} | {dto.Subject}");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["Email:SenderName"] ?? "Amore Mebel",
            _config["Email:SenderEmail"] ?? username));
        message.To.Add(new MailboxAddress(dto.ToName, dto.ToEmail));
        message.Subject = dto.Subject;

        var builder = new BodyBuilder();
        if (dto.IsHtml) builder.HtmlBody = dto.Body;
        else builder.TextBody = dto.Body;

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["Email:SmtpHost"] ?? "smtp.gmail.com",
            int.Parse(_config["Email:SmtpPort"] ?? "587"),
            SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    // ── Şifrə sıfırlama ──────────────────────────────────────────────────
    public async Task SendForgotPasswordAsync(string toEmail, string toName, string resetToken, string lang)
    {
        var frontendUrl = _config["App:FrontendUrl"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";

        var (subject, body) = lang switch
        {
            "ru" => ("Сброс пароля — Amore Mebel",
                BuildHtml(lang, $"Здравствуйте, {toName}!",
                    "Для сброса пароля нажмите кнопку ниже.",
                    "Ссылка действительна 1 час.",
                    resetLink, "Сбросить пароль")),
            "en" => ("Password Reset — Amore Mebel",
                BuildHtml(lang, $"Hello, {toName}!",
                    "Click the button below to reset your password.",
                    "The link is valid for 1 hour.",
                    resetLink, "Reset Password")),
            _ => ("Şifrə sıfırlama — Amore Mebel",
                BuildHtml(lang, $"Salam, {toName}!",
                    "Şifrənizi sıfırlamaq üçün aşağıdakı düyməni klikləyin.",
                    "Keçid 1 saat etibarlıdır.",
                    resetLink, "Şifrəni Sıfırla"))
        };

        await SendAsync(new SendEmailDto { ToEmail = toEmail, ToName = toName, Subject = subject, Body = body });
    }

    // ── Sifariş təsdiq (müştəriyə) ───────────────────────────────────────
    public async Task SendOrderConfirmationAsync(string toEmail, string toName, int orderId, decimal total, string lang)
    {
        var frontendUrl = _config["App:FrontendUrl"] ?? "http://localhost:5173";

        var (subject, heading, intro, totalLabel, ctaLabel) = lang switch
        {
            "ru" => ($"Заказ #{orderId} принят — Amore Mebel",
                $"Ваш заказ принят!",
                $"Здравствуйте, <strong>{toName}</strong>! Ваш заказ <strong>#{orderId}</strong> успешно оформлен. Наш менеджер свяжется с вами в ближайшее время для подтверждения доставки.",
                "Итого:", "Отследить заказ"),
            "en" => ($"Order #{orderId} Confirmed — Amore Mebel",
                "Your order is confirmed!",
                $"Hello, <strong>{toName}</strong>! Your order <strong>#{orderId}</strong> has been placed successfully. Our manager will contact you shortly to confirm delivery.",
                "Total:", "Track Order"),
            _ => ($"#{orderId} sifariş qəbul edildi — Amore Mebel",
                "Sifarişiniz qəbul edildi!",
                $"Salam, <strong>{toName}</strong>! <strong>#{orderId}</strong> nömrəli sifarişiniz uğurla qəbul edildi. Menecerlərimiz çatdırılmanı təsdiqləmək üçün sizinlə ən qısa zamanda əlaqə saxlayacaq.",
                "Ümumi məbləğ:", "Sifarişə bax")
        };

        var body = BuildOrderEmailHtml(heading, intro, new[] {
            ($"Sifariş №", $"#{orderId}"),
            (totalLabel, $"<strong style='color:#7A9E7E'>{total:F2} ₼</strong>"),
            ("Status:", lang == "ru" ? "Ожидает подтверждения" : lang == "en" ? "Pending confirmation" : "Təsdiq gözlənilir"),
        }, $"{frontendUrl}/profile", ctaLabel);

        await SendAsync(new SendEmailDto { ToEmail = toEmail, ToName = toName, Subject = subject, Body = body });
    }

    // ── Status dəyişikliyi (müştəriyə) ───────────────────────────────────
    public async Task SendOrderStatusChangedAsync(string toEmail, string toName, int orderId, string status, string lang)
    {
        var statusLocal = lang switch
        {
            "ru" => status switch
            {
                "Confirmed" => "Подтверждён",
                "InProgress" => "Готовится",
                "Delivered" => "Доставлен",
                "Cancelled" => "Отменён",
                _ => status
            },
            "en" => status switch
            {
                "Confirmed" => "Confirmed",
                "InProgress" => "In Progress",
                "Delivered" => "Delivered",
                "Cancelled" => "Cancelled",
                _ => status
            },
            _ => status switch
            {
                "Confirmed" => "Təsdiqləndi",
                "InProgress" => "Hazırlanır",
                "Delivered" => "Çatdırıldı",
                "Cancelled" => "Ləğv edildi",
                _ => status
            }
        };

        var (subject, heading, intro) = lang switch
        {
            "ru" => ($"Статус заказа #{orderId} обновлён — Amore Mebel",
                     "Статус заказа обновлён",
                     $"Здравствуйте, <strong>{toName}</strong>! Статус вашего заказа <strong>#{orderId}</strong> изменён."),
            "en" => ($"Order #{orderId} Status Updated — Amore Mebel",
                     "Your order status changed",
                     $"Hello, <strong>{toName}</strong>! The status of order <strong>#{orderId}</strong> has been updated."),
            _ => ($"#{orderId} sifarişin statusu dəyişdi — Amore Mebel",
                     "Sifariş statusu dəyişdi",
                     $"Salam, <strong>{toName}</strong>! <strong>#{orderId}</strong> nömrəli sifarişinizin statusu dəyişdi.")
        };

        var statusColor = status switch
        {
            "Delivered" => "#7A9E7E",
            "Cancelled" => "#C0392B",
            "Confirmed" => "#2980b9",
            _ => "#C9A84C"
        };

        var body = BuildOrderEmailHtml(heading, intro, new[] {
            ("Sifariş №:", $"#{orderId}"),
            ("Yeni status:", $"<span style='color:{statusColor};font-weight:600'>{statusLocal}</span>"),
        }, null, null);

        await SendAsync(new SendEmailDto { ToEmail = toEmail, ToName = toName, Subject = subject, Body = body });
    }

    // ── Admin bildirişi (yeni sifariş) ───────────────────────────────────
    public async Task SendAdminOrderNotificationAsync(
        int orderId, string customerName, string customerEmail,
        decimal total, string paymentMethod, string deliveryAddress, string lang)
    {
        var adminEmail = _config["Email:AdminEmail"]
                      ?? _config["SeedAdmin:Email"]
                      ?? "admin@furnitureshop.az";

        var subject = $"🛒 Yeni Sifariş #{orderId} — Amore Mebel";

        var body = BuildOrderEmailHtml(
            $"Yeni Sifariş #{orderId}",
            "Yeni bir sifariş daxil oldu. Aşağıdakı məlumatları yoxlayın.",
            new[] {
                ("Sifariş №:",    $"#{orderId}"),
                ("Müştəri:",      customerName),
                ("E-poçt:",       $"<a href='mailto:{customerEmail}'>{customerEmail}</a>"),
                ("Məbləğ:",       $"<strong style='color:#7A9E7E'>{total:F2} ₼</strong>"),
                ("Ödəniş:",       paymentMethod),
                ("Ünvan:",        deliveryAddress),
            },
            null, null,
            accentColor: "#C9A84C");

        await SendAsync(new SendEmailDto
        {
            ToEmail = adminEmail,
            ToName = "Amore Mebel Admin",
            Subject = subject,
            Body = body,
        });
    }

    // ── HTML Builder ─────────────────────────────────────────────────────
    private static string BuildHtml(string lang, string heading, string body1, string body2, string ctaUrl, string ctaLabel)
    {
        return $@"
{BaseTemplate($@"
<h1 style='font-family:Georgia,serif;font-size:28px;font-weight:400;color:#1C1C1C;margin:0 0 16px'>{heading}</h1>
<p style='font-size:14px;color:#555;line-height:1.7;margin:0 0 12px'>{body1}</p>
<p style='font-size:13px;color:#888;margin:0 0 28px'>{body2}</p>
<a href='{ctaUrl}' style='display:inline-block;padding:14px 32px;background:#1C1C1C;color:#fff;text-decoration:none;font-family:sans-serif;font-size:12px;letter-spacing:2px;text-transform:uppercase'>
  {ctaLabel}
</a>")}";
    }

    private static string BuildOrderEmailHtml(
        string heading, string intro,
        (string label, string value)[] rows,
        string? ctaUrl, string? ctaLabel,
        string accentColor = "#7A9E7E")
    {
        var rowsHtml = string.Join("", rows.Select(r => $@"
<tr>
  <td style='padding:10px 0;color:#888;font-size:13px;white-space:nowrap;vertical-align:top;padding-right:24px'>{r.label}</td>
  <td style='padding:10px 0;color:#1C1C1C;font-size:13px'>{r.value}</td>
</tr>"));

        var ctaHtml = (ctaUrl != null && ctaLabel != null)
            ? $@"<a href='{ctaUrl}' style='display:inline-block;padding:14px 32px;background:#1C1C1C;color:#fff;text-decoration:none;font-family:sans-serif;font-size:11px;letter-spacing:2px;text-transform:uppercase;margin-top:28px'>{ctaLabel} →</a>"
            : "";

        return BaseTemplate($@"
<div style='border-left:3px solid {accentColor};padding-left:16px;margin-bottom:28px'>
  <h1 style='font-family:Georgia,serif;font-size:26px;font-weight:400;color:#1C1C1C;margin:0 0 8px'>{heading}</h1>
  <p style='font-size:14px;color:#555;line-height:1.7;margin:0'>{intro}</p>
</div>
<table cellpadding='0' cellspacing='0' style='width:100%;border-collapse:collapse;background:#F7F3EE;padding:0'>
  <tr><td colspan='2' style='padding:4px 0'></td></tr>
  {rowsHtml}
  <tr><td colspan='2' style='padding:4px 0'></td></tr>
</table>
{ctaHtml}");
    }

    private static string BaseTemplate(string content) => $@"<!DOCTYPE html>
<html lang='az'>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f0ebe3;font-family:''DM Sans'',Arial,sans-serif'>
  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr>
      <td align='center' style='padding:40px 20px'>
        <table width='580' cellpadding='0' cellspacing='0' style='max-width:580px;width:100%'>
          <!-- Header -->
          <tr>
            <td style='background:#1C1C1C;padding:24px 36px;text-align:center'>
              <span style='font-family:Georgia,serif;font-size:22px;font-weight:400;color:#fff;letter-spacing:3px'>AMORE MEBEL</span>
            </td>
          </tr>
          <!-- Body -->
          <tr>
            <td style='background:#fff;padding:36px;border:1px solid #e8e1d9;border-top:none'>
              {content}
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td style='padding:20px 36px;text-align:center;background:#F7F3EE;border:1px solid #e8e1d9;border-top:none'>
              <p style='font-size:11px;color:#a0998f;margin:0;letter-spacing:1px'>
                © {DateTime.UtcNow.Year} Amore Mebel · Bakı, Azərbaycan
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
}