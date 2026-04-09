using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Enums;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class OrderReadRepository : GenericReadRepository<Order>, IOrderReadRepository
{
    public OrderReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
        => await Table
            .Where(x => x.UserId == userId)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Translations)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images.Where(img => img.IsPrimary))
            .Include(x => x.Items)
                .ThenInclude(i => i.Collection)
                    .ThenInclude(c => c!.Translations)
            .Include(x => x.Address)
            .Include(x => x.DeliveryInfo)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<Order?> GetWithDetailsAsync(int id)
        => await Table
            .Where(x => x.Id == id)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Translations)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .Include(x => x.Items)
                .ThenInclude(i => i.Collection)
                    .ThenInclude(c => c!.Translations)
            .Include(x => x.Address)
            .Include(x => x.DeliveryInfo)
            .Include(x => x.DiscountCode)
            .Include(x => x.User)
            .FirstOrDefaultAsync();

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = Table
            .Include(x => x.User)
            .Include(x => x.DeliveryInfo)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetByStatusPagedAsync(OrderStatus status, int page, int pageSize)
    {
        var query = Table
            .Where(x => x.Status == status)
            .Include(x => x.User)
            .Include(x => x.DeliveryInfo)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetByDateRangePagedAsync(DateTime from, DateTime to, int page, int pageSize)
    {
        var query = Table
            .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
            .Include(x => x.User)
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<int> GetTotalCountAsync()
        => Table.CountAsync();

    public async Task<decimal> GetTotalRevenueAsync()
    {
        var sum = await Table
            .Where(x => x.Status != OrderStatus.Cancelled)
            .SumAsync(x => (decimal?)x.TotalPrice);
        return sum ?? 0m;
    }

    public Task<int> GetCountByStatusAsync(OrderStatus status)
        => Table.CountAsync(x => x.Status == status);

    public async Task<IEnumerable<(int ProductId, string ProductName, string? ImageUrl, string? Category, decimal Price, int Stock, int SoldCount)>>
        GetTopProductsAsync(int limit = 5)
    {
        var result = await Table
            .Where(x => x.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .Where(i => i.Product != null)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                SoldCount = g.Sum(i => i.Quantity),
                Product   = g.First().Product
            })
            .OrderByDescending(x => x.SoldCount)
            .Take(limit)
            .ToListAsync();

        return result.Select(x => (
            x.ProductId ?? 0,
            x.Product!.Translations.FirstOrDefault()?.Name ?? $"Product #{x.ProductId}",
            x.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? x.Product.Images.FirstOrDefault()?.ImageUrl,
            x.Product.FurnitureCategory?.Translations.FirstOrDefault()?.Name,
            x.Product.Price,
            x.Product.Stock,
            x.SoldCount
        ));
    }

    public async Task<IEnumerable<(int Year, int Month, decimal Revenue, int OrderCount)>>
        GetMonthlyRevenueAsync(int year)
    {
        var data = await Table
            .Where(x => x.CreatedAt.Year == year && x.Status != OrderStatus.Cancelled)
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue    = g.Sum(x => x.TotalPrice),
                OrderCount = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return data.Select(x => (x.Year, x.Month, x.Revenue, x.OrderCount));
    }
}
