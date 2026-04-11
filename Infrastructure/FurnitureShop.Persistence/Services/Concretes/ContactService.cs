using FurnitureShop.Application.Dtos.Contact;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text;
using System.Text.Json;

namespace FurnitureShop.Persistence.Services.Concretes;

/// <summary>
/// Contact form məlumatlarını:
///   1) Admin emailinə göndərir (IEmailService vasitəsilə)
///   2) Admin telefon nömrəsinə WhatsApp/SMS API vasitəsilə göndərir
///      (appsettings-dəki "Contact:AdminPhone" konfiqurasi olduqda)
/// </summary>
public class ContactService : IContactService
{
    private readonly IEmailService   _email;
    private readonly IConfiguration  _config;
    private static readonly ILogger _log = Log.ForContext<ContactService>();

    public ContactService(IEmailService email, IConfiguration config)
    {
        _email  = email;
        _config = config;
    }

    public async Task SendContactMessageAsync(ContactMessageDto dto)
    {
        var lang = dto.Lang ?? "az";

        // ── 1. Admin emailinə bildiriş ──────────────────────────────────
        try
        {
            await _email.SendContactNotificationAsync(
                fromName:  dto.Name,
                fromEmail: dto.Email,
                fromPhone: dto.Phone,
                subject:   dto.Subject,
                message:   dto.Message,
                lang:      lang);

            _log.Information("Contact email göndərildi — From: {Email}", dto.Email);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Contact email göndərilə bilmədi — From: {Email}", dto.Email);
            // email xətası cavab qaytarmağı bloklamasın
        }

        // ── 2. Admin telefona SMS / WhatsApp ─────────────────────────────
        // appsettings.json-da "Contact:AdminPhone" varsa göndər
        var adminPhone = _config["Contact:AdminPhone"];
        if (!string.IsNullOrWhiteSpace(adminPhone))
        {
            try
            {
                await SendSmsNotificationAsync(adminPhone, dto);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Contact SMS göndərilə bilmədi — AdminPhone: {Phone}", adminPhone);
            }
        }
    }

    /// <summary>
    /// SMS göndərmək üçün iki seçim dəstəklənir:
    ///   A) Twilio  — appsettings: Twilio:AccountSid + Twilio:AuthToken + Twilio:FromPhone
    ///   B) Eskiz   — appsettings: Eskiz:Email + Eskiz:Password + Eskiz:From  (AZ lokal)
    ///
    /// Heç biri konfiqurasiya edilməyibsə, xəbərdarlıq logu yazılır.
    /// </summary>
    private async Task SendSmsNotificationAsync(string toPhone, ContactMessageDto dto)
    {
        var smsText = BuildSmsText(dto);

        // ── A. Twilio ───────────────────────────────────────────────────
        var twilioSid   = _config["Twilio:AccountSid"];
        var twilioToken = _config["Twilio:AuthToken"];
        var twilioFrom  = _config["Twilio:FromPhone"];

        if (!string.IsNullOrWhiteSpace(twilioSid) &&
            !string.IsNullOrWhiteSpace(twilioToken) &&
            !string.IsNullOrWhiteSpace(twilioFrom))
        {
            await SendViaTwilioAsync(twilioSid, twilioToken, twilioFrom, toPhone, smsText);
            _log.Information("Twilio SMS göndərildi — To: {Phone}", toPhone);
            return;
        }

        // ── B. Eskiz SMS (Azerbaijan) ───────────────────────────────────
        var eskizEmail    = _config["Eskiz:Email"];
        var eskizPassword = _config["Eskiz:Password"];
        var eskizFrom     = _config["Eskiz:From"] ?? "4546";

        if (!string.IsNullOrWhiteSpace(eskizEmail) && !string.IsNullOrWhiteSpace(eskizPassword))
        {
            await SendViaEskizAsync(eskizEmail, eskizPassword, eskizFrom, toPhone, smsText);
            _log.Information("Eskiz SMS göndərildi — To: {Phone}", toPhone);
            return;
        }

        _log.Warning(
            "Contact SMS göndərilmədi — Nə Twilio nə Eskiz konfiqurasiya edilməyib. " +
            "appsettings.json-a 'Twilio' və ya 'Eskiz' bölməsini əlavə edin. AdminPhone: {Phone}", toPhone);
    }

    private static string BuildSmsText(ContactMessageDto dto)
    {
        var sb = new StringBuilder();
        sb.Append("Amore Mebel - Yeni Muraciet!\n");
        sb.Append($"Ad: {dto.Name}\n");
        sb.Append($"Email: {dto.Email}\n");
        if (!string.IsNullOrWhiteSpace(dto.Phone))
            sb.Append($"Tel: {dto.Phone}\n");
        if (!string.IsNullOrWhiteSpace(dto.Subject))
            sb.Append($"Movzu: {dto.Subject}\n");
        // SMS limit — mesajı 100 simvola qədər kəs
        var msg = dto.Message?.Length > 100
            ? dto.Message[..100] + "..."
            : dto.Message;
        sb.Append($"Mesaj: {msg}");
        return sb.ToString();
    }

    // ── Twilio REST API ───────────────────────────────────────────────────
    private static async Task SendViaTwilioAsync(
        string accountSid, string authToken, string from, string to, string body)
    {
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"]   = to,
            ["From"] = from,
            ["Body"] = body,
        });

        var response = await client.PostAsync(
            $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json",
            content);

        response.EnsureSuccessStatusCode();
    }

    // ── Eskiz.uz REST API (AZ/UZ region) ─────────────────────────────────
    private static async Task SendViaEskizAsync(
        string email, string password, string from, string to, string text)
    {
        using var client = new HttpClient();

        // 1. Token al
        var tokenResp = await client.PostAsync(
            "https://notify.eskiz.uz/api/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"]    = email,
                ["password"] = password,
            }));
        tokenResp.EnsureSuccessStatusCode();

        var tokenJson = await tokenResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(tokenJson);
        var token = doc.RootElement
            .GetProperty("data")
            .GetProperty("token")
            .GetString() ?? throw new Exception("Eskiz token alına bilmədi");

        // 2. SMS göndər
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Azerbaycan nömrəsi üçün country_code = 994
        var smsResp = await client.PostAsync(
            "https://notify.eskiz.uz/api/message/sms/send",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["mobile_phone"] = to.TrimStart('+'),
                ["message"]      = text,
                ["from"]         = from,
                ["callback_url"] = "",
            }));
        smsResp.EnsureSuccessStatusCode();
    }
}
