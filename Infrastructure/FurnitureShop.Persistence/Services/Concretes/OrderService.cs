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
using FurnitureShop.Persistence.Datas;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly AppDbContext                 _db;
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
        IMapper                      mapper,
        AppDbContext                 db)
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
        _db                 = db;
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

        var user    = await _userManager.FindByIdAsync(userId);
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

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = _mapper.Map<Order>(dto);
            order.UserId        = userId;
            order.Status        = OrderStatus.Pending;
            order.PaymentStatus = PaymentStatus.Pending;

            if (dto.DeliveryInfo is not null)
                order.DeliveryInfo = _mapper.Map<DeliveryInfo>(dto.DeliveryInfo);

            decimal discountAmount = 0;
            decimal subtotal       = 0;
            var itemPrices = new Dictionary<int, decimal>();

            foreach (var item in dto.Items)
            {
                if (item.ProductId.HasValue)
                {
                    var product = await _productReadRepo.GetByIdAsync(item.ProductId.Value);
                    if (product is null)
                        throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));

                    if (!dto.IsCustomOrder && product.Stock < item.Quantity)
                        throw new Application.Exceptions.ValidationException(
                            new Dictionary<string, List<string>>
                            {
                                { "stock", new List<string> { $"'{product.Translations.FirstOrDefault()?.Name ?? "Məhsul"}' üçün kifayət qədər stok yoxdur. Mövcud: {product.Stock}" } }
                            });

                    itemPrices[item.ProductId.Value] = product.Price;
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

                if (discount.Status != DiscountStatus.Active ||
                    (discount.ExpiresAt.HasValue && discount.ExpiresAt < DateTime.UtcNow))
                    throw new Application.Exceptions.ValidationException(
                        new Dictionary<string, List<string>> { { "discountCode",
                            new List<string> { ValidationMessages.Get(Lang, "DiscountCodeExpired") } } });

                if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
                    throw new Application.Exceptions.ValidationException(
                        new Dictionary<string, List<string>> { { "discountCode",
                            new List<string> { ValidationMessages.Get(Lang, "DiscountCodeUsedUp") } } });

                if (discount.MinOrderAmount.HasValue && subtotal < discount.MinOrderAmount.Value)
                    throw new Application.Exceptions.ValidationException(
                        new Dictionary<string, List<string>> { { "discountCode",
                            new List<string> { string.Format(ValidationMessages.Get(Lang, "DiscountCodeMinAmount"), discount.MinOrderAmount.Value) } } });

                discountAmount = discount.Type == DiscountType.Percent
                    ? Math.Round(subtotal * discount.Value / 100, 2)
                    : discount.Value;
                discountAmount = Math.Min(discountAmount, subtotal);

                discount.UsedCount++;
                _discountWriteRepo.Update(discount);
            }

            const decimal freeShippingThreshold = 500m;
            const decimal shippingCost          = 15m;
            order.ShippingCost   = subtotal >= freeShippingThreshold ? 0 : shippingCost;
            order.DiscountAmount = discountAmount;
            order.TotalPrice     = subtotal - discountAmount + order.ShippingCost;

            foreach (var orderItem in order.Items)
            {
                if (orderItem.ProductId.HasValue && itemPrices.TryGetValue(orderItem.ProductId.Value, out var price))
                    orderItem.UnitPrice = price;
                else if (orderItem.CollectionId.HasValue)
                {
                    var col = await _collectionReadRepo.GetByIdAsync(orderItem.CollectionId.Value);
                    if (col is not null) orderItem.UnitPrice = col.TotalPrice;
                }
            }

            await _writeRepo.AddAsync(order);
            await _writeRepo.SaveChangesAsync();

            if (!dto.IsCustomOrder)
            {
                foreach (var item in dto.Items.Where(i => i.ProductId.HasValue))
                {
                    var product = await _productReadRepo.GetByIdAsync(item.ProductId!.Value);
                    if (product is not null)
                    {
                        product.Stock -= item.Quantity;
                        _productWriteRepo.Update(product);
                    }
                }
                try
                {
                    await _productWriteRepo.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    _log.Warning("Stok race condition — OrderId: {OrderId} UserId: {UserId}", order.Id, userId);
                    throw new Application.Exceptions.ValidationException(
                        new Dictionary<string, List<string>> { { "stock",
                            new List<string> { "Stok məlumatı dəyişdi, zəhmət olmasa yenidən cəhd edin" } } });
                }
            }

            var cart = await _cartReadRepo.GetByUserIdAsync(userId);
            if (cart is not null)
            {
                cart.Items.Clear();
                _cartWriteRepo.Update(cart);
                await _cartWriteRepo.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            _log.Information("Sifariş uğurla yaradıldı — OrderId: {OrderId} TotalPrice: {TotalPrice} IsCustom: {IsCustom}",
                order.Id, order.TotalPrice, order.IsCustomOrder);

            var capturedOrderId       = order.Id;
            var capturedTotal         = order.TotalPrice;
            var capturedLang          = Lang;
            var capturedMethod        = order.PaymentMethod;
            var capturedNote          = order.Note;
            var capturedIsCustom      = order.IsCustomOrder;
            var capturedCustomDesc    = order.CustomDescription;

            _ = Task.Run(async () =>
            {
                try
                {
                    var userEntity = await _userManager.FindByIdAsync(userId);

                    if (userEntity?.Email is not null)
                    {
                        await _emailService.SendOrderConfirmationAsync(
                            userEntity.Email,
                            $"{userEntity.Name} {userEntity.Surname}",
                            capturedOrderId, capturedTotal, capturedLang);
                    }

                    var payMethodLabel = capturedMethod switch
                    {
                        PaymentMethod.CashOnDelivery => "Nağd",
                        PaymentMethod.Card           => "Kart",
                        PaymentMethod.BankTransfer   => "Bank köçürməsi",
                        PaymentMethod.Installment    => "Kredit/Taksit",
                        PaymentMethod.PartialCard    => "İlkin ödəniş + qalan nağd",
                        _                            => capturedMethod.ToString()
                    };

                    await _emailService.SendAdminOrderNotificationAsync(
                        capturedOrderId,
                        userEntity is not null ? $"{userEntity.Name} {userEntity.Surname}" : "—",
                        userEntity?.Email ?? "—",
                        userEntity?.PhoneNumber ?? "—",
                        capturedTotal,
                        payMethodLabel,
                        capturedNote ?? "—",
                        capturedIsCustom,
                        capturedCustomDesc,
                        capturedLang);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Email göndərməsi uğursuz — OrderId: {OrderId}", capturedOrderId);
                }
            });

            return order.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
            throw new ForbiddenException(ValidationMessages.Get(Lang, "OrderCancelForbidden"));

        if (order.Status == OrderStatus.Cancelled)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "order",
                    new List<string> { ValidationMessages.Get(Lang, "OrderAlreadyCancelled") } } });

        if (order.Status == OrderStatus.Delivered)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "order",
                    new List<string> { ValidationMessages.Get(Lang, "OrderAlreadyDelivered") } } });

        if (!isAdmin && order.Status == OrderStatus.Confirmed)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "order",
                    new List<string> { ValidationMessages.Get(Lang, "OrderAlreadyConfirmed") } } });

        if (!isAdmin && order.Status == OrderStatus.InProgress)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "order",
                    new List<string> { ValidationMessages.Get(Lang, "OrderAlreadyInProgress") } } });

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            order.Status = OrderStatus.Cancelled;
            _writeRepo.Update(order);
            await _writeRepo.SaveChangesAsync();

            if (!order.IsCustomOrder)
            {
                foreach (var item in order.Items.Where(i => i.ProductId.HasValue))
                {
                    var product = await _productReadRepo.GetByIdAsync(item.ProductId!.Value);
                    if (product is not null)
                    {
                        product.Stock += item.Quantity;
                        _productWriteRepo.Update(product);
                    }
                }
                if (order.Items.Any(i => i.ProductId.HasValue))
                    await _productWriteRepo.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

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

    public async Task UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
    {
        var order = await _readRepo.GetWithDetailsAsync(id);
        if (order is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "OrderNotFound"));

        order.Status = dto.Status;
        if (dto.AdminNote is not null)
            order.AdminNote = dto.AdminNote;

        if (dto.EstimatedDeliveryDate.HasValue)
            order.EstimatedDeliveryDate = dto.EstimatedDeliveryDate;

        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Sifariş statusu dəyişdirildi — OrderId: {OrderId} Status: {Status} AdminNote: {Note}",
            id, dto.Status, dto.AdminNote);

        if (order.User?.Email is not null)
        {
            var capturedUser     = order.User;
            var capturedId       = order.Id;
            var capturedStatus   = dto.Status;
            var capturedNote     = dto.AdminNote;
            var capturedDelivery = dto.EstimatedDeliveryDate;
            var capturedLang     = Lang;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderStatusChangedAsync(
                        capturedUser.Email!,
                        $"{capturedUser.Name} {capturedUser.Surname}",
                        capturedId,
                        capturedStatus.ToString(),
                        capturedNote,
                        capturedDelivery,
                        capturedLang);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Status email uğursuz — OrderId: {OrderId}", capturedId);
                }
            });
        }
    }

    public async Task MarkPaymentPaidAsync(int orderId)
    {
        var order = await _readRepo.GetByIdAsync(orderId);
        if (order is null) return;

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            _log.Information("MarkPaymentPaid — artıq ödənilib — OrderId: {OrderId}", orderId);
            return;
        }

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

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            _log.Warning("MarkPaymentFailed — artıq ödənilib, dəyişdirilmir — OrderId: {OrderId}", orderId);
            return;
        }

        order.PaymentStatus = PaymentStatus.Failed;
        _writeRepo.Update(order);
        await _writeRepo.SaveChangesAsync();
        _log.Warning("Ödəniş uğursuz — OrderId: {OrderId}", orderId);
    }
}
