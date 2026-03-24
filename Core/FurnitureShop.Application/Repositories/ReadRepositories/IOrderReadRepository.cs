using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IOrderReadRepository : IGenericReadRepository<Order>
{
    //user
    Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
    Task<Order?> GetWithDetailsAsync(int id);

    // Admin — paged
    Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetByStatusPagedAsync(OrderStatus status, int page, int pageSize);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetByDateRangePagedAsync(DateTime from, DateTime to, int page, int pageSize);

    // Admin dashboard
    Task<int> GetTotalCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<int> GetCountByStatusAsync(OrderStatus status);
}
