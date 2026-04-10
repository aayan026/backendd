using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Payment;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/payments")]
public class PaymentController : BaseApiController
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService   _orderService;

    public PaymentController(IPaymentService paymentService, IOrderService orderService)
    {
        _paymentService = paymentService;
        _orderService   = orderService;
    }

    /// <summary>
    /// Kartla ödəniş üçün Payriff ödəniş URL-i yaradır.
    /// Əvvəlcə POST /api/orders ilə sifariş yarat, sonra bu endpoint-i çağır.
    /// </summary>
    [Authorize]
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
    {
        // GetOrderDetailsAsync daxilindən ForbiddenException — başqa userin sifarişi
        var order = await _orderService.GetOrderDetailsAsync(dto.OrderId, UserId);
        if (order is null)
            return NotFound();

        // Artıq ödənilmiş sifariş üçün yenidən ödəniş başlatma
        if (order.PaymentStatus == PaymentStatus.Paid)
            return BadRequest(ApiResponse<object>.ValidationError(
                new Dictionary<string, List<string>> { { "payment",
                    new List<string> { Msg("PaymentAlreadyPaid") } } },
                Msg("ValidationError")));

        var description = $"Amore Mebel - Sifariş #{dto.OrderId}";
        var lang        = Request.Headers["Accept-Language"].FirstOrDefault() ?? "az";

        var paymentUrl = await _paymentService.InitiateAsync(
            dto.OrderId, order.TotalPrice, description, lang);

        return OkResponse(new { paymentUrl });
    }

    /// <summary>
    /// Payriff redirect-dən sonra frontend bu endpoint-i çağırır.
    /// SECURITY FIX: orderId-nin bu usera aid olduğu GetOrderDetailsAsync ilə yoxlanır.
    /// Əgər başqa user bu orderId-ni göndərsə — 403 Forbidden qaytarılır.
    /// </summary>
    [Authorize]
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentDto dto)
    {
        // Ownership yoxlaması — GetOrderDetailsAsync ForbiddenException atar əgər başqa user
        var order = await _orderService.GetOrderDetailsAsync(dto.OrderId, UserId);
        if (order is null)
            return NotFound();

        // Idempotency — artıq ödənilibsə uğurlu cavab qaytar, yenidən process etmə
        if (order.PaymentStatus == PaymentStatus.Paid)
            return OkResponse(new { success = true });

        var verified = await _paymentService.VerifyAsync(dto.PayriffOrderId, dto.SessionId);

        if (verified)
            await _orderService.MarkPaymentPaidAsync(dto.OrderId);
        else
            await _orderService.MarkPaymentFailedAsync(dto.OrderId);

        return OkResponse(new { success = verified });
    }
}
