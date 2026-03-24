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
                    .ThenInclude(p => p!.Images.Where(img => img.IsPrimary))
            .Include(x => x.Items)
                .ThenInclude(i => i.Collection)
            .Include(x => x.Address)
            .Include(x => x.DeliveryInfo)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<Order?> GetWithDetailsAsync(int id)
        => await Table
            .Where(x => x.Id == id)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .Include(x => x.Items)
                .ThenInclude(i => i.Collection)
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
}
