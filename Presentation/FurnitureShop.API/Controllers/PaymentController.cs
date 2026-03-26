using FurnitureShop.Application.Dtos.Payment;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/payments")]
public class PaymentController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Stripe PaymentIntent yaradır — client-side ödəniş üçün clientSecret qaytarır
    /// </summary>
    [Authorize]
    [HttpPost("create-intent")]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentDto dto)
        => OkResponse(await _paymentService.CreatePaymentIntentAsync(dto));

    /// <summary>
    /// Ödənişi server-side təsdiqləyir
    /// </summary>
    [Authorize]
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentDto dto)
    {
        var success = await _paymentService.ConfirmPaymentAsync(dto);
        return OkResponse(new { success });
    }

    /// <summary>
    /// Stripe Webhook — ödəniş statusunu avtomatik idarə edir
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var payload   = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(signature))
            return BadRequest("Missing Stripe-Signature header.");

        try
        {
            await _paymentService.HandleWebhookAsync(payload, signature);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
