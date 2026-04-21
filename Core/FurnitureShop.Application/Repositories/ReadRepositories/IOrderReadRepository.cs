using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IOrderReadRepository : IGenericReadRepository<Order>
{
    //user
    Task<IEnumerable<Order>> GetByUserIdAsync(string userId, string lang);
    Task<Order?> GetWithDetailsAsync(int id, string lang);
    // Admin — paged
    Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetByStatusPagedAsync(OrderStatus status, int page, int pageSize);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetByDateRangePagedAsync(DateTime from, DateTime to, int page, int pageSize);

    // Admin dashboard — stats
    Task<int>     GetTotalCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<int>     GetCountByStatusAsync(OrderStatus status);

    // Admin dashboard — top products
    Task<IEnumerable<(int ProductId, string ProductName, string? ImageUrl, string? Category, decimal Price, int Stock, int SoldCount)>>
        GetTopProductsAsync(int limit = 5);

    // Admin dashboard — monthly revenue
    Task<IEnumerable<(int Year, int Month, decimal Revenue, int OrderCount)>>
        GetMonthlyRevenueAsync(int year);
}
