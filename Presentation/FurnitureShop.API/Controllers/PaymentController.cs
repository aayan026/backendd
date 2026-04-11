using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/payments")]
public class PaymentController : BaseApiController
{
    private readonly IOrderService _orderService;

    public PaymentController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Admin: sifarişi ödənilmiş kimi işarələ
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{orderId}/mark-paid")]
    public async Task<IActionResult> MarkPaid(int orderId)
    {
        await _orderService.MarkPaymentPaidAsync(orderId);
        return OkResponse(new { success = true });
    }

    /// <summary>
    /// Admin: sifarişi ödənilməmiş / uğursuz kimi işarələ
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{orderId}/mark-failed")]
    public async Task<IActionResult> MarkFailed(int orderId)
    {
        await _orderService.MarkPaymentFailedAsync(orderId);
        return OkResponse(new { success = true });
    }
}
