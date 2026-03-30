using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;
    private static readonly string[] _allowedRoles = { "Admin", "Customer" };

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ── Dashboard ──────────────────────────────────────────────────────────

    /// <summary>
    /// Tam dashboard məlumatı (köhnə frontend uyğunluğu üçün saxlanıldı)
    /// GET /api/admin/dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
        => OkResponse(await _adminService.GetDashboardAsync());

    /// <summary>
    /// Dashboard statistikaları — frontend adminApi.getStats() üçün
    /// GET /api/admin/dashboard/stats
    /// Response: { totalProducts, totalOrders, todayOrders, totalRevenue, outOfStock, totalCustomers }
    /// </summary>
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> DashboardStats()
    {
        var data = await _adminService.GetDashboardAsync();
        return OkResponse(new
        {
            totalOrders    = data.Orders.Total,
            todayOrders    = data.Orders.Pending,   // Pending ≈ bugünkü yeni sifarişlər
            totalRevenue   = data.Revenue,
            totalCustomers = data.UserCount,
            pendingOrders  = data.Orders.Pending,
            confirmedOrders  = data.Orders.Confirmed,
            inProgressOrders = data.Orders.InProgress,
            deliveredOrders  = data.Orders.Delivered,
            cancelledOrders  = data.Orders.Cancelled
        });
    }

    /// <summary>
    /// Ən çox satılan məhsullar
    /// GET /api/admin/dashboard/top-products?limit=5
    /// </summary>
    [HttpGet("dashboard/top-products")]
    public async Task<IActionResult> TopProducts([FromQuery] int limit = 5)
        => OkResponse(await _adminService.GetTopProductsAsync(limit));

    /// <summary>
    /// Aylıq gəlir statistikası
    /// GET /api/admin/dashboard/monthly-revenue?year=2025
    /// </summary>
    [HttpGet("dashboard/monthly-revenue")]
    public async Task<IActionResult> MonthlyRevenue([FromQuery] int? year)
        => OkResponse(await _adminService.GetMonthlyRevenueAsync(year ?? DateTime.UtcNow.Year));

    // ── Users ──────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationParams pagination)
    {
        var result = await _adminService.GetUsersAsync(pagination);
        return Ok(ApiResponse<object>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUser(string userId)
        => OkResponse(await _adminService.GetUserByIdAsync(userId));

    [HttpPatch("users/{userId}/ban")]
    public async Task<IActionResult> BanUser(string userId)
    {
        await _adminService.BanUserAsync(userId);
        return UpdatedResponse();
    }

    [HttpPatch("users/{userId}/unban")]
    public async Task<IActionResult> UnbanUser(string userId)
    {
        await _adminService.UnbanUserAsync(userId);
        return UpdatedResponse();
    }

    [HttpPatch("users/{userId}/role")]
    public async Task<IActionResult> ChangeRole(string userId, [FromQuery] string role)
    {
        if (string.IsNullOrWhiteSpace(role) || !_allowedRoles.Contains(role))
            return BadRequest(ApiResponse<object>.ValidationError(
                new Dictionary<string, List<string>>
                {
                    { "role", new List<string> { $"Rol '{role}' etibarsızdır. İcazə verilənlər: {string.Join(", ", _allowedRoles)}" } }
                },
                Msg("ValidationError")));

        await _adminService.ChangeUserRoleAsync(userId, role);
        return UpdatedResponse();
    }
}
