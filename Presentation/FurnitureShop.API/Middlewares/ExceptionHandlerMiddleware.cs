using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using System.Text.Json;

namespace FurnitureShop.API.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly Dictionary<string, Dictionary<string, string>> _fieldNames = new()
    {
        ["az"] = new()
        {
            ["Name"] = "Ad",
            ["Surname"] = "Soyad",
            ["Email"] = "Email",
            ["Password"] = "Şifrə",
            ["Price"] = "Qiymət",
            ["PriceExtra"] = "Əlavə qiymət",
            ["Stock"] = "Stok",
            ["CategoryId"] = "Kateqoriya",
            ["Colors"] = "Rənglər",
            ["ColorName"] = "Rəng adı",
            ["Images"] = "Şəkillər",
            ["Translations"] = "Tərcümələr",
            ["LangCode"] = "Dil kodu",
            ["TotalPrice"] = "Ümumi qiymət",
            ["DiscountPrice"] = "Endirimli qiymət",
            ["Products"] = "Məhsullar",
            ["Items"] = "Elementlər",
            ["Quantity"] = "Miqdar",
            ["DeliveryInfo"] = "Çatdırılma məlumatı",
            ["ScheduledDate"] = "Tarix",
            ["TimeSlot"] = "Vaxt aralığı",
            ["AddressId"] = "Ünvan",
            ["HexCode"] = "Hex kodu",
            ["Item"] = "Element",
            ["Id"] = "Id",
            ["Value"] = "Dəyər",
            ["MinOrderAmount"] = "Minimum məbləğ",
            ["MaxUses"] = "Maksimum istifadə",
            ["ExpiresAt"] = "Bitmə tarixi",
        },
        ["ru"] = new()
        {
            ["Name"] = "Имя",
            ["Surname"] = "Фамилия",
            ["Email"] = "Email",
            ["Password"] = "Пароль",
            ["Price"] = "Цена",
            ["PriceExtra"] = "Доп. цена",
            ["Stock"] = "Склад",
            ["CategoryId"] = "Категория",
            ["Colors"] = "Цвета",
            ["ColorName"] = "Цвет",
            ["Images"] = "Фото",
            ["Translations"] = "Переводы",
            ["LangCode"] = "Код языка",
            ["TotalPrice"] = "Общая цена",
            ["DiscountPrice"] = "Цена со скидкой",
            ["Products"] = "Товары",
            ["Items"] = "Элементы",
            ["Quantity"] = "Количество",
            ["DeliveryInfo"] = "Доставка",
            ["ScheduledDate"] = "Дата",
            ["TimeSlot"] = "Время",
            ["AddressId"] = "Адрес",
            ["HexCode"] = "Hex код",
            ["Item"] = "Элемент",
            ["Id"] = "Id",
            ["Value"] = "Значение",
            ["MinOrderAmount"] = "Мин. сумма",
            ["MaxUses"] = "Макс. использований",
            ["ExpiresAt"] = "Срок действия",
        },
        ["en"] = new()
        {
            ["Name"] = "Name",
            ["Surname"] = "Surname",
            ["Email"] = "Email",
            ["Password"] = "Password",
            ["Price"] = "Price",
            ["PriceExtra"] = "Extra price",
            ["Stock"] = "Stock",
            ["CategoryId"] = "Category",
            ["Colors"] = "Colors",
            ["ColorName"] = "Color name",
            ["Images"] = "Images",
            ["Translations"] = "Translations",
            ["LangCode"] = "Language code",
            ["TotalPrice"] = "Total price",
            ["DiscountPrice"] = "Discount price",
            ["Products"] = "Products",
            ["Items"] = "Items",
            ["Quantity"] = "Quantity",
            ["DeliveryInfo"] = "Delivery info",
            ["ScheduledDate"] = "Scheduled date",
            ["TimeSlot"] = "Time slot",
            ["AddressId"] = "Address",
            ["HexCode"] = "Hex code",
            ["Item"] = "Item",
            ["Id"] = "Id",
            ["Value"] = "Value",
            ["MinOrderAmount"] = "Min amount",
            ["MaxUses"] = "Max uses",
            ["ExpiresAt"] = "Expiry date",
        }
    };

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        IWebHostEnvironment env,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _env = env;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        using var scope = _scopeFactory.CreateScope();
        var langService = scope.ServiceProvider.GetRequiredService<ILanguageService>();
        var lang = langService.GetCurrentLanguage();
        var traceId = context.TraceIdentifier;

        ApiResponse<object> response;

        switch (exception)
        {
            case FluentValidation.ValidationException fvEx:
                context.Response.StatusCode = 422;
                var fvErrors = fvEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => LocalizeField(g.Key, lang),
                        g => g.Select(e => LocalizeMessage(e.ErrorMessage, lang)).ToList());
                response = ApiResponse<object>.ValidationError(fvErrors, ValidationMessages.Get(lang, "ValidationError"));
                break;

            case Application.Exceptions.ValidationException validationEx:
                context.Response.StatusCode = 422;
                response = ApiResponse<object>.ValidationError(
                    LocalizeErrors(validationEx.Errors, lang),
                    ValidationMessages.Get(lang, "ValidationError"));
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = 404;
                response = ApiResponse<object>.NotFound(LocalizeMessage(notFoundEx.Message, lang));
                break;

            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = 401;
                response = ApiResponse<object>.Unauthorized(LocalizeMessage(unauthorizedEx.Message, lang));
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = 403;
                response = ApiResponse<object>.Forbidden(LocalizeMessage(forbiddenEx.Message, lang));
                break;

            default:
                context.Response.StatusCode = 500;
                var serverMessage = _env.IsDevelopment()
                    ? exception.Message
                    : ValidationMessages.Get(lang, "ServerError");
                response = ApiResponse<object>.ServerError(serverMessage, traceId);
                break;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string LocalizeMessage(string message, string lang)
    {
        var parts = message.Split('|');
        var key = parts[0];
        var args = parts.Skip(1).Select(a => LocalizeField(a, lang)).ToArray();
        return ValidationMessages.Get(lang, key, args);
    }

    private static Dictionary<string, List<string>> LocalizeErrors(
        Dictionary<string, List<string>> errors, string lang)
        => errors.ToDictionary(
            kvp => LocalizeField(kvp.Key, lang),
            kvp => kvp.Value.Select(m => LocalizeMessage(m, lang)).ToList());

    private static string LocalizeField(string field, string lang)
    {
        if (_fieldNames.TryGetValue(lang, out var dict) && dict.TryGetValue(field, out var localized))
            return localized;
        if (_fieldNames["az"].TryGetValue(field, out var fallback))
            return fallback;
        return field;
    }
}

public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlerMiddleware>();
}
