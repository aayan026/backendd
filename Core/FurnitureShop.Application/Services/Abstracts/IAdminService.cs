using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Admin;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();
    Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int limit = 5);
    Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int year);
    Task<int> GetTotalProductCountAsync();

    Task<PagedList<AdminUserDto>> GetUsersAsync(PaginationParams pagination);
    Task<AdminUserDto> GetUserByIdAsync(string userId);
    Task BanUserAsync(string userId);
    Task UnbanUserAsync(string userId);
    Task ChangeUserRoleAsync(string userId, string role);
}