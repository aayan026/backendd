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

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Dashboard — order statistikaları + revenue + user sayı
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
        => OkResponse(await _adminService.GetDashboardAsync());

    // ── User Management ────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationParams pagination)
    {
        var result = await _adminService.GetUsersAsync(pagination);
        return Ok(Application.Common.Responses.ApiResponse<object>.Ok(result.Items, result.Pagination, Msg("Success")));
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
        await _adminService.ChangeUserRoleAsync(userId, role);
        return UpdatedResponse();
    }
}
