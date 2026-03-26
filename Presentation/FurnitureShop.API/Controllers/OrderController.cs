using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Order;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/orders")]
[Authorize]
public class OrderController : BaseApiController
{
    private readonly IOrderService _service;

    public OrderController(IOrderService service)
    {
        _service = service;
    }

    // ── Müştəri ────────────────────────────────────────────────────────────

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
        => OkResponse(await _service.GetUserOrdersAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetOrderDetailsAsync(id, UserId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var id = await _service.CreateAsync(dto, UserId);
        return CreatedResponse(new { id });
    }

    /// <summary>
    /// Sifarişi ləğv et — PATCH istifadə edilir (Cancel bir dəyişiklikdir, silmə deyil)
    /// </summary>
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var message = await _service.CancelAsync(id, UserId);
        return OkResponse<object>(new { message });
    }

    // ── Admin ──────────────────────────────────────────────────────────────

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetAllAsync(pagination);
        return Ok(ApiResponse<List<OrderDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/by-status")]
    public async Task<IActionResult> GetByStatus([FromQuery] OrderStatus status, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetByStatusAsync(status, pagination);
        return Ok(ApiResponse<List<OrderDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/by-date")]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetByDateRangeAsync(from, to, pagination);
        return Ok(ApiResponse<List<OrderDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        await _service.UpdateStatusAsync(id, dto.Status);
        return UpdatedResponse();
    }
}
