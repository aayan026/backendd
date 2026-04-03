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
using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace FurnitureShop.Persistence.Services.Concretes;

public class OrderService : IOrderService
{
    private readonly IOrderReadRepository _readRepo;
    private readonly IOrderWriteRepository _writeRepo;
    private readonly ICartWriteRepository _cartWriteRepo;
    private readonly ICartReadRepository _cartReadRepo;
    private readonly IDiscountCodeReadRepository _discountReadRepo;
    private readonly IDiscountCodeWriteRepository _discountWriteRepo;
    private readonly ILanguageService _langService;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    private string Lang => _langService.GetCurrentLanguage();

    public OrderService(
        IOrderReadRepository readRepo,
        IOrderWriteRepository writeRepo,
        ICartWriteRepository cartWriteRepo,
        ICartReadRepository cartReadRepo,
        IDiscountCodeReadRepository discountReadRepo,
        IDiscountCodeWriteRepository discountWriteRepo,
        ILanguageService langService,
        IEmailService emailService,
        UserManager<AppUser> userManager,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _cartWriteRepo = cartWriteRepo;
        _cartReadRepo = cartReadRepo;
        _discountReadRepo = discountReadRepo;
        _discountWriteRepo = discountWriteRepo;
        _langService = langService;
        _emailService = emailService;
        _userManager = userManager;
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
        order.PaymentStatus = PaymentStatus.Pending;

        if (dto.DeliveryInfo is not null)
            order.DeliveryInfo = _mapper.Map<DeliveryInfo>(dto.DeliveryInfo);

        // Discount UsedCount artır
        if (dto.DiscountCodeId.HasValue)
        {
            var discount = await _discountReadRepo.GetByIdAsync(dto.DiscountCodeId.Value);
            if (discount is not null)
            {
                discount.UsedCount++;
                _discountWriteRepo.Update(discount);
            }
        }

        await _writeRepo.AddAsync(order);
        await _writeRepo.SaveChangesAsync();

        // Cart-ı təmizlə
        var cart = await _cartReadRepo.GetByUserIdAsync(userId);
        if (cart is not null)
        {
            cart.Items.Clear();
            _cartWriteRepo.Update(cart);
            await _cartWriteRepo.SaveChangesAsync();
        }

        // Order confirmation email → müştəriyə
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            _ = _emailService.SendOrderConfirmationAsync(
                user.Email!, $"{user.Name} {user.Surname}",
                order.Id, order.TotalPrice, Lang);
        }

        // Yeni sifariş bildirişi → adminə
        var payMethodLabel = order.PaymentMethod switch
        {
            PaymentMethod.CashOnDelivery => "Nağd",
            PaymentMethod.Card => "Kart",
            PaymentMethod.BankTransfer => "Bank köçürməsi",
            _ => order.PaymentMethod.ToString()
        };
        var deliveryAddr = order.Note ?? "—";

        _ = _emailService.SendAdminOrderNotificationAsync(
            order.Id,
            user is not null ? $"{user.Name} {user.Surname}" : "—",
            user?.Email ?? "—",
            order.TotalPrice,
            payMethodLabel,
            deliveryAddr,
            Lang);

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
        var order = await _readRepo.GetWithDetailsAsync(id);

        if (order is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        order.Status = status;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        // Status dəyişikliyi emaili
        if (order.User is not null)
        {
            _ = _emailService.SendOrderStatusChangedAsync(
                order.User.Email!, $"{order.User.Name} {order.User.Surname}",
                order.Id, status.ToString(), Lang);
        }
    }

    // ── Payment ────────────────────────────────────────────────────────────

    public async Task MarkPaymentPaidAsync(int orderId)
    {
        var order = await _readRepo.GetByIdAsync(orderId);
        if (order is null) return;

        order.PaymentStatus = PaymentStatus.Paid;
        if (order.Status == OrderStatus.Pending)
            order.Status = OrderStatus.Confirmed;

        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task MarkPaymentFailedAsync(int orderId)
    {
        var order = await _readRepo.GetByIdAsync(orderId);
        if (order is null) return;

        order.PaymentStatus = PaymentStatus.Failed;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();
    }
}