using AutoMapper;
using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Order;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Persistence.Services.Concretes;

public class OrderService : IOrderService
{
    private readonly IOrderReadRepository _readRepo;
    private readonly IOrderWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;

    private string Lang => _langService.GetCurrentLanguage();

    public OrderService(
        IOrderReadRepository readRepo,
        IOrderWriteRepository writeRepo,
        ILanguageService langService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
    }

    // ── Müştəri ────────────────────────────────────────────────────────────

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _readRepo.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetOrderDetailsAsync(int id, string userId)
    {
        var order = await _readRepo.GetWithDetailsAsync(id);

        if (order is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        if (order.UserId != userId)
            throw new ForbiddenException(ValidationMessages.Get(Lang, "OrderAccessForbidden"));

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<int> CreateAsync(CreateOrderDto dto, string userId)
    {
        var order = _mapper.Map<Order>(dto);
        order.UserId = userId;
        order.Status = OrderStatus.Pending;

        if (dto.DeliveryInfo is not null)
            order.DeliveryInfo = _mapper.Map<DeliveryInfo>(dto.DeliveryInfo);

        await _writeRepo.AddAsync(order);
        await _writeRepo.SaveChangesAsync();
        return order.Id;
    }

    public async Task<string> CancelAsync(int id, string userId)
    {
        var order = await _readRepo.GetByIdAsync(id);

        if (order is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        if (order.UserId != userId)
            throw new ForbiddenException(ValidationMessages.Get(Lang, "OrderCancelForbidden"));

        if (order.Status == OrderStatus.Cancelled)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "order", new List<string> { ValidationMessages.Get(Lang, "OrderAlreadyCancelled") } }
                });

        if (order.Status == OrderStatus.Delivered)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "order", new List<string> { ValidationMessages.Get(Lang, "OrderAlreadyDelivered") } }
                });

        order.Status = OrderStatus.Cancelled;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        return ValidationMessages.Get(Lang, "OrderCancelled");
    }

    // ── Admin ──────────────────────────────────────────────────────────────

    public async Task<PagedList<OrderDto>> GetAllAsync(PaginationParams pagination)
    {
        var (items, total) = await _readRepo.GetAllPagedAsync(pagination.Page, pagination.PageSize);
        return new PagedList<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(items),
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task<PagedList<OrderDto>> GetByStatusAsync(OrderStatus status, PaginationParams pagination)
    {
        var (items, total) = await _readRepo.GetByStatusPagedAsync(status, pagination.Page, pagination.PageSize);
        return new PagedList<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(items),
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task<PagedList<OrderDto>> GetByDateRangeAsync(DateTime from, DateTime to, PaginationParams pagination)
    {
        var (items, total) = await _readRepo.GetByDateRangePagedAsync(from, to, pagination.Page, pagination.PageSize);
        return new PagedList<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(items),
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await _readRepo.GetByIdAsync(id);

        if (order is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        order.Status = status;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();
    }
}
