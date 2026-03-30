using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Admin;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Enums;
using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Services.Concretes;

public class AdminService : IAdminService
{
    private readonly UserManager<AppUser>   _userManager;
    private readonly IOrderReadRepository   _orderReadRepo;
    private readonly ILanguageService       _langService;

    private string Lang => _langService.GetCurrentLanguage();

    public AdminService(
        UserManager<AppUser>   userManager,
        IOrderReadRepository   orderReadRepo,
        ILanguageService       langService)
    {
        _userManager    = userManager;
        _orderReadRepo  = orderReadRepo;
        _langService    = langService;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var userCount = await _userManager.Users.CountAsync();
        var revenue   = await _orderReadRepo.GetTotalRevenueAsync();

        return new AdminDashboardDto
        {
            Revenue   = revenue,
            UserCount = userCount,
            Orders    = new OrderStatsDto
            {
                Total      = await _orderReadRepo.GetTotalCountAsync(),
                Pending    = await _orderReadRepo.GetCountByStatusAsync(OrderStatus.Pending),
                Confirmed  = await _orderReadRepo.GetCountByStatusAsync(OrderStatus.Confirmed),
                InProgress = await _orderReadRepo.GetCountByStatusAsync(OrderStatus.InProgress),
                Delivered  = await _orderReadRepo.GetCountByStatusAsync(OrderStatus.Delivered),
                Cancelled  = await _orderReadRepo.GetCountByStatusAsync(OrderStatus.Cancelled)
            }
        };
    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int limit = 5)
    {
        var rows = await _orderReadRepo.GetTopProductsAsync(limit);
        return rows.Select(r => new TopProductDto
        {
            Id        = r.ProductId,
            Name      = r.ProductName,
            ImageUrl  = r.ImageUrl,
            Category  = r.Category,
            Price     = r.Price,
            Stock     = r.Stock,
            SoldCount = r.SoldCount
        });
    }

    public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int year)
    {
        var rows = await _orderReadRepo.GetMonthlyRevenueAsync(year);
        return rows.Select(r => new MonthlyRevenueDto
        {
            Year    = r.Year,
            Month   = r.Month,
            Revenue = r.Revenue,
            Orders  = r.OrderCount
        });
    }

    public async Task<PagedList<AdminUserDto>> GetUsersAsync(PaginationParams pagination)
    {
        var total = await _userManager.Users.CountAsync();
        var users = await _userManager.Users
            .OrderByDescending(u => u.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var dtos = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles   = await _userManager.GetRolesAsync(user);
            var lockout = await _userManager.IsLockedOutAsync(user);
            dtos.Add(new AdminUserDto
            {
                Id          = user.Id,
                Name        = user.Name,
                Surname     = user.Surname,
                Email       = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                IsLocked    = lockout,
                Roles       = roles
            });
        }

        return new PagedList<AdminUserDto>
        {
            Items      = dtos,
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task<AdminUserDto> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        var roles   = await _userManager.GetRolesAsync(user);
        var lockout = await _userManager.IsLockedOutAsync(user);

        return new AdminUserDto
        {
            Id= user.Id,
            Name= user.Name,
            Surname= user.Surname,
            Email= user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            IsLocked= lockout,
            Roles= roles
        };
    }

    public async Task BanUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
    }

    public async Task UnbanUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
    }

    public async Task ChangeUserRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);
    }
}
