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
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class OrderService : IOrderService
{
    private readonly IOrderReadRepository        _readRepo;
    private readonly IOrderWriteRepository       _writeRepo;
    private readonly ICartWriteRepository        _cartWriteRepo;
    private readonly ICartReadRepository         _cartReadRepo;
    private readonly IDiscountCodeReadRepository  _discountReadRepo;
    private readonly IDiscountCodeWriteRepository _discountWriteRepo;
    private readonly ILanguageService            _langService;
    private readonly IEmailService               _emailService;
    private readonly UserManager<AppUser>        _userManager;
    private readonly IMapper                     _mapper;
    private static readonly ILogger _log = Log.ForContext<OrderService>();

    private string Lang => _langService.GetCurrentLanguage();

    public OrderService(
        IOrderReadRepository        readRepo,
        IOrderWriteRepository       writeRepo,
        ICartWriteRepository        cartWriteRepo,
        ICartReadRepository         cartReadRepo,
        IDiscountCodeReadRepository  discountReadRepo,
        IDiscountCodeWriteRepository discountWriteRepo,
        ILanguageService            langService,
        IEmailService               emailService,
        UserManager<AppUser>        userManager,
        IMapper                     mapper)
    {
        _readRepo          = readRepo;
        _writeRepo         = writeRepo;
        _cartWriteRepo     = cartWriteRepo;
        _cartReadRepo      = cartReadRepo;
        _discountReadRepo  = discountReadRepo;
        _discountWriteRepo = discountWriteRepo;
        _langService       = langService;
        _emailService      = emailService;
        _userManager       = userManager;
        _mapper            = mapper;
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
    {
        _log.Information("İstifadəçinin sifarişləri sorğusu — UserId: {UserId}", userId);
        var orders = await _readRepo.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetOrderDetailsAsync(int id, string userId)
    {
        _log.Information("Sifariş detalları sorğusu — OrderId: {OrderId} UserId: {UserId}", id, userId);

        var order = await _readRepo.GetWithDetailsAsync(id);
        if (order is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        if (order.UserId != userId)
        {
            _log.Warning("Sifarişə icazəsiz giriş cəhdi — OrderId: {OrderId} UserId: {UserId}", id, userId);
            throw new ForbiddenException(ValidationMessages.Get(Lang, "OrderAccessForbidden"));
        }

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<int> CreateAsync(CreateOrderDto dto, string userId)
    {
        _log.Information("Yeni sifariş yaradılır — UserId: {UserId} MəhsulSayı: {ItemCount} ÜmumiQiymət: {TotalPrice} ÖdəməMetodu: {PaymentMethod}",
            userId, dto.Items?.Count ?? 0, dto.TotalPrice, dto.PaymentMethod);

        var order = _mapper.Map<Order>(dto);
        order.UserId        = userId;
        order.Status        = OrderStatus.Pending;
        order.PaymentStatus = PaymentStatus.Pending;

        if (dto.DeliveryInfo is not null)
            order.DeliveryInfo = _mapper.Map<DeliveryInfo>(dto.DeliveryInfo);

        if (dto.DiscountCodeId.HasValue)
        {
            var discount = await _discountReadRepo.GetByIdAsync(dto.DiscountCodeId.Value);
            if (discount is not null)
            {
                discount.UsedCount++;
                _discountWriteRepo.Update(discount);
                _log.Information("Endirim kodu tətbiq edildi — DiscountCodeId: {DiscountCodeId} UsedCount: {UsedCount}",
                    discount.Id, discount.UsedCount);
            }
        }

        await _writeRepo.AddAsync(order);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Sifariş uğurla yaradıldı — OrderId: {OrderId} UserId: {UserId} ÜmumiQiymət: {TotalPrice}",
            order.Id, userId, order.TotalPrice);

        // Cart-ı təmizlə
        var cart = await _cartReadRepo.GetByUserIdAsync(userId);
        if (cart is not null)
        {
            var cartItemCount = cart.Items.Count;
            cart.Items.Clear();
            _cartWriteRepo.Update(cart);
            await _cartWriteRepo.SaveChangesAsync();
            _log.Information("Sifariş sonrası səbət təmizləndi — UserId: {UserId} SilənMəhsulSayı: {Count}", userId, cartItemCount);
        }

        // Email — müştəriyə
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            _ = _emailService.SendOrderConfirmationAsync(
                user.Email!, $"{user.Name} {user.Surname}",
                order.Id, order.TotalPrice, Lang);
            _log.Information("Sifariş təsdiq emaili göndərildi — UserId: {UserId} Email: {Email} OrderId: {OrderId}",
                userId, user.Email, order.Id);
        }

        // Email — adminə
        var payMethodLabel = order.PaymentMethod switch
        {
            PaymentMethod.CashOnDelivery => "Nağd",
            PaymentMethod.Card           => "Kart",
            PaymentMethod.BankTransfer   => "Bank köçürməsi",
            _                            => order.PaymentMethod.ToString()
        };

        _ = _emailService.SendAdminOrderNotificationAsync(
            order.Id,
            user is not null ? $"{user.Name} {user.Surname}" : "—",
            user?.Email ?? "—",
            order.TotalPrice,
            payMethodLabel,
            order.Note ?? "—",
            Lang);

        return order.Id;
    }

    public async Task<string> CancelAsync(int id, string userId)
    {
        _log.Information("Sifariş ləğv edilmə tələbi — OrderId: {OrderId} UserId: {UserId}", id, userId);

        var order = await _readRepo.GetByIdAsync(id);
        if (order is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        if (order.UserId != userId)
        {
            _log.Warning("Sifariş ləğvinə icazəsiz cəhd — OrderId: {OrderId} UserId: {UserId}", id, userId);
            throw new ForbiddenException(ValidationMessages.Get(Lang, "OrderCancelForbidden"));
        }

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

        var oldStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Sifariş ləğv edildi — OrderId: {OrderId} UserId: {UserId} EskiStatus: {OldStatus}",
            id, userId, oldStatus);

        return ValidationMessages.Get(Lang, "OrderCancelled");
    }

    public async Task<PagedList<OrderDto>> GetAllAsync(PaginationParams pagination)
    {
        _log.Information("Admin — Bütün sifarişlər sorğusu — Səhifə: {Page} ÖlçüsU: {PageSize}", pagination.Page, pagination.PageSize);
        var (items, total) = await _readRepo.GetAllPagedAsync(pagination.Page, pagination.PageSize);
        return new PagedList<OrderDto>
        {
            Items      = _mapper.Map<List<OrderDto>>(items),
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task<PagedList<OrderDto>> GetByStatusAsync(OrderStatus status, PaginationParams pagination)
    {
        _log.Information("Admin — Statusa görə sifarişlər — Status: {Status}", status);
        var (items, total) = await _readRepo.GetByStatusPagedAsync(status, pagination.Page, pagination.PageSize);
        return new PagedList<OrderDto>
        {
            Items      = _mapper.Map<List<OrderDto>>(items),
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task<PagedList<OrderDto>> GetByDateRangeAsync(DateTime from, DateTime to, PaginationParams pagination)
    {
        _log.Information("Admin — Tarix aralığına görə sifarişlər — {From} — {To}", from, to);
        var (items, total) = await _readRepo.GetByDateRangePagedAsync(from, to, pagination.Page, pagination.PageSize);
        return new PagedList<OrderDto>
        {
            Items      = _mapper.Map<List<OrderDto>>(items),
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await _readRepo.GetWithDetailsAsync(id);
        if (order is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        var oldStatus = order.Status;
        order.Status = status;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Sifariş statusu dəyişdirildi — OrderId: {OrderId} EskiStatus: {OldStatus} YeniStatus: {NewStatus}",
            id, oldStatus, status);

        if (order.User is not null)
        {
            _ = _emailService.SendOrderStatusChangedAsync(
                order.User.Email!, $"{order.User.Name} {order.User.Surname}",
                order.Id, status.ToString(), Lang);
            _log.Information("Status dəyişikliyi emaili göndərildi — OrderId: {OrderId} Email: {Email}", id, order.User.Email);
        }
    }

    public async Task MarkPaymentPaidAsync(int orderId)
    {
        var order = await _readRepo.GetByIdAsync(orderId);
        if (order is null) return;

        order.PaymentStatus = PaymentStatus.Paid;
        if (order.Status == OrderStatus.Pending)
            order.Status = OrderStatus.Confirmed;

        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Ödəniş uğurlu — OrderId: {OrderId} YeniStatus: {Status}", orderId, order.Status);
    }

    public async Task MarkPaymentFailedAsync(int orderId)
    {
        var order = await _readRepo.GetByIdAsync(orderId);
        if (order is null) return;

        order.PaymentStatus = PaymentStatus.Failed;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        _log.Warning("Ödəniş uğursuz — OrderId: {OrderId}", orderId);
    }
}
