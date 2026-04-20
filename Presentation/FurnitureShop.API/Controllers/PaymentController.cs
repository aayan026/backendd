using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/payments")]
public class PaymentController : BaseApiController
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;

    public PaymentController(IPaymentService paymentService, IOrderService orderService)
    {
        _paymentService = paymentService;
        _orderService = orderService;
    }

    [Authorize]
    [HttpPost("{orderId:int}/create-intent")]
    public async Task<IActionResult> CreateIntent(int orderId)
    {
        var result = await _paymentService.CreatePaymentIntentAsync(orderId, UserId);
        return OkResponse(result);
    }


    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        await _paymentService.HandleWebhookAsync(payload, signature);
        return Ok();
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("{orderId:int}/mark-paid")]
    public async Task<IActionResult> MarkPaid(int orderId)
    {
        await _orderService.MarkPaymentPaidAsync(orderId);
        return OkResponse(new { success = true, message = "Ödəniş uğurlu kimi işarələndi." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{orderId:int}/mark-failed")]
    public async Task<IActionResult> MarkFailed(int orderId)
    {
        await _orderService.MarkPaymentFailedAsync(orderId);
        return OkResponse(new { success = true, message = "Ödəniş uğursuz kimi işarələndi." });
    }
}