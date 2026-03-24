using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;

    public AdminController(IOrderService orderService, IProductService productService)
    {
        _orderService = orderService;
        _productService = productService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var allOrders = await _orderService.GetAllAsync(new Application.Common.Responses.PaginationParams
        {
            Page = 1,
            PageSize = 1
        });

        var pendingOrders = await _orderService.GetByStatusAsync(
            OrderStatus.Pending,
            new Application.Common.Responses.PaginationParams { Page = 1, PageSize = 1 });

        var confirmedOrders = await _orderService.GetByStatusAsync(
            OrderStatus.Confirmed,
            new Application.Common.Responses.PaginationParams { Page = 1, PageSize = 1 });

        var deliveredOrders = await _orderService.GetByStatusAsync(
            OrderStatus.Delivered,
            new Application.Common.Responses.PaginationParams { Page = 1, PageSize = 1 });

        var cancelledOrders = await _orderService.GetByStatusAsync(
            OrderStatus.Cancelled,
            new Application.Common.Responses.PaginationParams { Page = 1, PageSize = 1 });

        return OkResponse(new
        {
            orders = new
            {
                total      = allOrders.Pagination.TotalCount,
                pending    = pendingOrders.Pagination.TotalCount,
                confirmed  = confirmedOrders.Pagination.TotalCount,
                delivered  = deliveredOrders.Pagination.TotalCount,
                cancelled  = cancelledOrders.Pagination.TotalCount
            }
        });
    }
}
