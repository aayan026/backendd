using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Order;
using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
    Task<OrderDto?> GetOrderDetailsAsync(int id, string userId);
    Task<int> CreateAsync(CreateOrderDto dto, string userId);
    Task<string> CancelAsync(int id, string userId);

    
    Task<PagedList<OrderDto>> GetAllAsync(PaginationParams pagination);
    Task<PagedList<OrderDto>> GetByStatusAsync(OrderStatus status, PaginationParams pagination);
    Task<PagedList<OrderDto>> GetByDateRangeAsync(DateTime from, DateTime to, PaginationParams pagination);
    Task UpdateStatusAsync(int id, UpdateOrderStatusDto dto);

    Task MarkPaymentPaidAsync(int orderId);
    Task MarkPaymentFailedAsync(int orderId);
}
