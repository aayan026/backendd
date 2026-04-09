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
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class AdminService : IAdminService
{
    private readonly UserManager<AppUser>   _userManager;
    private readonly IOrderReadRepository   _orderReadRepo;
    private readonly IProductReadRepository _productReadRepo;
    private readonly ILanguageService       _langService;
    private static readonly ILogger _log = Log.ForContext<AdminService>();

    private string Lang => _langService.GetCurrentLanguage();

    public AdminService(
        UserManager<AppUser>   userManager,
        IOrderReadRepository   orderReadRepo,
        IProductReadRepository productReadRepo,
        ILanguageService       langService)
    {
        _userManager     = userManager;
        _orderReadRepo   = orderReadRepo;
        _productReadRepo = productReadRepo;
        _langService     = langService;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        _log.Information("Admin dashboard sorğusu");
        var userCount = await _userManager.Users.CountAsync();
        var revenue   = await _orderReadRepo.GetTotalRevenueAsync();
        _log.Information("Dashboard məlumatları — İstifadəçi: {UserCount} Gəlir: {Revenue}", userCount, revenue);

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

    public async Task<int> GetTotalProductCountAsync()
    {
        var count = await _productReadRepo.GetAll().CountAsync();
        _log.Information("Ümumi məhsul sayı sorğusu — Say: {Count}", count);
        return count;
    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int limit = 5)
    {
        _log.Information("Ən çox satılan məhsullar sorğusu — Limit: {Limit}", limit);
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
        _log.Information("Aylıq gəlir sorğusu — İl: {Year}", year);
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
        _log.Information("Admin — İstifadəçilər siyahısı sorğusu — Səhifə: {Page}", pagination.Page);
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
        _log.Information("Admin — İstifadəçi detalı sorğusu — UserId: {UserId}", userId);
        var user    = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));
        var roles   = await _userManager.GetRolesAsync(user);
        var lockout = await _userManager.IsLockedOutAsync(user);

        return new AdminUserDto
        {
            Id          = user.Id,
            Name        = user.Name,
            Surname     = user.Surname,
            Email       = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            IsLocked    = lockout,
            Roles       = roles
        };
    }

    public async Task BanUserAsync(string userId)
    {
        _log.Warning("Admin — İstifadəçi banlama — UserId: {UserId}", userId);
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        _log.Warning("İstifadəçi banlandı — UserId: {UserId} Email: {Email}", userId, user.Email);
    }

    public async Task UnbanUserAsync(string userId)
    {
        _log.Information("Admin — İstifadəçi banının açılması — UserId: {UserId}", userId);
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        _log.Information("İstifadəçi banı açıldı — UserId: {UserId} Email: {Email}", userId, user.Email);
    }

    public async Task ChangeUserRoleAsync(string userId, string role)
    {
        _log.Information("Admin — İstifadəçi rolu dəyişdirilir — UserId: {UserId} YeniRol: {Role}", userId, role);
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);
        _log.Information("İstifadəçi rolu dəyişdirildi — UserId: {UserId} EskiRol: {OldRoles} YeniRol: {NewRole}", userId, currentRoles, role);
    }
}
