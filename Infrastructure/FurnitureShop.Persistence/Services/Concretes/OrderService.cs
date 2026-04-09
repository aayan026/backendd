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
    private readonly IOrderReadRepository         _readRepo;
    private readonly IOrderWriteRepository        _writeRepo;
    private readonly ICartWriteRepository         _cartWriteRepo;
    private readonly ICartReadRepository          _cartReadRepo;
    private readonly IDiscountCodeReadRepository  _discountReadRepo;
    private readonly IDiscountCodeWriteRepository _discountWriteRepo;
    private readonly IProductReadRepository       _productReadRepo;
    private readonly IProductWriteRepository      _productWriteRepo;
    private readonly ICollectionReadRepository    _collectionReadRepo;
    private readonly ILanguageService             _langService;
    private readonly IEmailService                _emailService;
    private readonly UserManager<AppUser>         _userManager;
    private readonly IMapper                      _mapper;
    private static readonly ILogger _log = Log.ForContext<OrderService>();

    private string Lang => _langService.GetCurrentLanguage();

    public OrderService(
        IOrderReadRepository         readRepo,
        IOrderWriteRepository        writeRepo,
        ICartWriteRepository         cartWriteRepo,
        ICartReadRepository          cartReadRepo,
        IDiscountCodeReadRepository  discountReadRepo,
        IDiscountCodeWriteRepository discountWriteRepo,
        IProductReadRepository       productReadRepo,
        IProductWriteRepository      productWriteRepo,
        ICollectionReadRepository    collectionReadRepo,
        ILanguageService             langService,
        IEmailService                emailService,
        UserManager<AppUser>         userManager,
        IMapper                      mapper)
    {
        _readRepo           = readRepo;
        _writeRepo          = writeRepo;
        _cartWriteRepo      = cartWriteRepo;
        _cartReadRepo       = cartReadRepo;
        _discountReadRepo   = discountReadRepo;
        _discountWriteRepo  = discountWriteRepo;
        _productReadRepo    = productReadRepo;
        _productWriteRepo   = productWriteRepo;
        _collectionReadRepo = collectionReadRepo;
        _langService        = langService;
        _emailService       = emailService;
        _userManager        = userManager;
        _mapper             = mapper;
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

        // Admin istənilən sifarişə baxa bilər
        var user = await _userManager.FindByIdAsync(userId);
        var isAdmin = user is not null && await _userManager.IsInRoleAsync(user, "Admin");

        if (!isAdmin && order.UserId != userId)
        {
            _log.Warning("Sifarişə icazəsiz giriş cəhdi — OrderId: {OrderId} UserId: {UserId}", id, userId);
            throw new ForbiddenException(ValidationMessages.Get(Lang, "OrderAccessForbidden"));
        }

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<int> CreateAsync(CreateOrderDto dto, string userId)
    {
        _log.Information("Yeni sifariş yaradılır — UserId: {UserId} MəhsulSayı: {ItemCount}",
            userId, dto.Items?.Count ?? 0);

        var order = _mapper.Map<Order>(dto);
        order.UserId        = userId;
        order.Status        = OrderStatus.Pending;
        order.PaymentStatus = PaymentStatus.Pending;

        if (dto.DeliveryInfo is not null)
            order.DeliveryInfo = _mapper.Map<DeliveryInfo>(dto.DeliveryInfo);

        // ── 1. Discount kodu yoxla və tətbiq et ─────────────────────────
        decimal discountAmount = 0;
        decimal subtotal       = 0;

        // İlk öncə subtotal-ı hesablayaq (discount üçün lazım)
        var itemPrices = new Dictionary<int, (decimal Price, int Stock)>();
        foreach (var item in dto.Items)
        {
            if (item.ProductId.HasValue)
            {
                var product = await _productReadRepo.GetByIdAsync(item.ProductId.Value);
                if (product is null)
                    throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));
                if (product.Stock < item.Quantity)
                    throw new Application.Exceptions.ValidationException(
                        new Dictionary<string, List<string>>
                        {
                            { "stock", new List<string> { $"'{product.Translations.FirstOrDefault()?.Name ?? "Məhsul"}' üçün kifayət qədər stok yoxdur. Mövcud: {product.Stock}" } }
                        });
                itemPrices[item.ProductId.Value] = (product.Price, product.Stock);
                subtotal += product.Price * item.Quantity;
            }
            else if (item.CollectionId.HasValue)
            {
                var collection = await _collectionReadRepo.GetByIdAsync(item.CollectionId.Value);
                if (collection is null)
                    throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionNotFound"));
                subtotal += collection.TotalPrice * item.Quantity;
            }
        }

        if (dto.DiscountCodeId.HasValue)
        {
            var discount = await _discountReadRepo.GetByIdAsync(dto.DiscountCodeId.Value);
            if (discount is null)
                throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));

            if (discount.Status != DiscountStatus.Active || (discount.ExpiresAt.HasValue && discount.ExpiresAt < DateTime.UtcNow))
                throw new Application.Exceptions.ValidationException(
                    new Dictionary<string, List<string>> { { "discountCode", new List<string> { ValidationMessages.Get(Lang, "DiscountCodeExpired") } } });

            if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
                throw new Application.Exceptions.ValidationException(
                    new Dictionary<string, List<string>> { { "discountCode", new List<string> { ValidationMessages.Get(Lang, "DiscountCodeUsedUp") } } });

            if (discount.MinOrderAmount.HasValue && subtotal < discount.MinOrderAmount.Value)
                throw new Application.Exceptions.ValidationException(
                    new Dictionary<string, List<string>> { { "discountCode", new List<string> { string.Format(ValidationMessages.Get(Lang, "DiscountCodeMinAmount"), discount.MinOrderAmount.Value) } } });

            discountAmount = discount.Type == DiscountType.Percent
                ? Math.Round(subtotal * discount.Value / 100, 2)
                : discount.Value;

            discountAmount = Math.Min(discountAmount, subtotal);

            discount.UsedCount++;
            _discountWriteRepo.Update(discount);
            _log.Information("Endirim kodu tətbiq edildi — DiscountCodeId: {DiscountCodeId} Endirim: {Amount}", discount.Id, discountAmount);
        }

        // ── 2. Çatdırılma qiyməti ────────────────────────────────────────
        const decimal freeShippingThreshold = 500m;
        const decimal shippingCost          = 15m;
        order.ShippingCost   = subtotal >= freeShippingThreshold ? 0 : shippingCost;
        order.DiscountAmount = discountAmount;
        order.TotalPrice     = subtotal - discountAmount + order.ShippingCost;

        // ── 3. OrderItem-lərə UnitPrice set et (tarixçə qorunur) ─────────
        foreach (var orderItem in order.Items)
        {
            if (orderItem.ProductId.HasValue && itemPrices.TryGetValue(orderItem.ProductId.Value, out var info))
                orderItem.UnitPrice = info.Price;
            else if (orderItem.CollectionId.HasValue)
            {
                var col = await _collectionReadRepo.GetByIdAsync(orderItem.CollectionId.Value);
                if (col is not null) orderItem.UnitPrice = col.TotalPrice;
            }
        }

        await _writeRepo.AddAsync(order);
        await _writeRepo.SaveChangesAsync();

        // ── 4. Stock azalt ───────────────────────────────────────────────
        foreach (var item in dto.Items.Where(i => i.ProductId.HasValue))
        {
            var product = await _productReadRepo.GetByIdAsync(item.ProductId!.Value);
            if (product is not null)
            {
                product.Stock -= item.Quantity;
                _productWriteRepo.Update(product);
            }
        }
        await _productWriteRepo.SaveChangesAsync();

        _log.Information("Sifariş uğurla yaradıldı — OrderId: {OrderId} TotalPrice: {TotalPrice}", order.Id, order.TotalPrice);

        // ── 5. Cart-ı təmizlə ─────────────────────────────────────────────
        var cart = await _cartReadRepo.GetByUserIdAsync(userId);
        if (cart is not null)
        {
            cart.Items.Clear();
            _cartWriteRepo.Update(cart);
            await _cartWriteRepo.SaveChangesAsync();
        }

        // ── 6. Email bildirişləri ─────────────────────────────────────────
        var userEntity = await _userManager.FindByIdAsync(userId);
        if (userEntity is not null)
        {
            _ = _emailService.SendOrderConfirmationAsync(
                userEntity.Email!, $"{userEntity.Name} {userEntity.Surname}",
                order.Id, order.TotalPrice, Lang);
        }

        var payMethodLabel = order.PaymentMethod switch
        {
            PaymentMethod.CashOnDelivery => "Nağd",
            PaymentMethod.Card           => "Kart",
            PaymentMethod.BankTransfer   => "Bank köçürməsi",
            _                            => order.PaymentMethod.ToString()
        };
        _ = _emailService.SendAdminOrderNotificationAsync(
            order.Id,
            userEntity is not null ? $"{userEntity.Name} {userEntity.Surname}" : "—",
            userEntity?.Email ?? "—",
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

        var user    = await _userManager.FindByIdAsync(userId);
        var isAdmin = user is not null && await _userManager.IsInRoleAsync(user, "Admin");

        if (!isAdmin && order.UserId != userId)
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

        order.Status = OrderStatus.Cancelled;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Sifariş ləğv edildi — OrderId: {OrderId}", id);
        return ValidationMessages.Get(Lang, "OrderCancelled");
    }

    public async Task<PagedList<OrderDto>> GetAllAsync(PaginationParams pagination)
    {
        var (items, total) = await _readRepo.GetAllPagedAsync(pagination.Page, pagination.PageSize);
        return new PagedList<OrderDto>
        {
            Items      = _mapper.Map<List<OrderDto>>(items),
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task<PagedList<OrderDto>> GetByStatusAsync(OrderStatus status, PaginationParams pagination)
    {
        var (items, total) = await _readRepo.GetByStatusPagedAsync(status, pagination.Page, pagination.PageSize);
        return new PagedList<OrderDto>
        {
            Items      = _mapper.Map<List<OrderDto>>(items),
            Pagination = new PaginationMeta(pagination.Page, pagination.PageSize, total)
        };
    }

    public async Task<PagedList<OrderDto>> GetByDateRangeAsync(DateTime from, DateTime to, PaginationParams pagination)
    {
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

        order.Status = status;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Sifariş statusu dəyişdirildi — OrderId: {OrderId} YeniStatus: {Status}", id, status);

        if (order.User is not null)
        {
            _ = _emailService.SendOrderStatusChangedAsync(
                order.User.Email!, $"{order.User.Name} {order.User.Surname}",
                order.Id, status.ToString(), Lang);
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
        _log.Information("Ödəniş uğurlu — OrderId: {OrderId}", orderId);
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
