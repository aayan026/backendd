using AutoMapper;
using FurnitureShop.Application.Dtos.Cart;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class CartService : ICartService
{
    private readonly ICartReadRepository     _readRepo;
    private readonly ICartWriteRepository    _writeRepo;
    private readonly IProductReadRepository  _productReadRepo;
    private readonly ICollectionReadRepository _collectionReadRepo;
    private readonly ILanguageService        _langService;
    private readonly IMapper                 _mapper;
    private static readonly ILogger _log = Log.ForContext<CartService>();

    private string lang => _langService.GetCurrentLanguage();

    public CartService(
        ICartReadRepository      readRepo,
        ICartWriteRepository     writeRepo,
        IProductReadRepository   productReadRepo,
        ICollectionReadRepository collectionReadRepo,
        ILanguageService         langService,
        IMapper                  mapper)
    {
        _readRepo           = readRepo;
        _writeRepo          = writeRepo;
        _productReadRepo    = productReadRepo;
        _collectionReadRepo = collectionReadRepo;
        _langService        = langService;
        _mapper             = mapper;
    }

    public async Task<CartDto> GetAsync(string userId)
    {
        _log.Information("Səbət sorğusu — UserId: {UserId}", userId);
        var cart = await _readRepo.GetByUserIdAsync(userId, lang);
        if (cart is null)
            return new CartDto();
        return _mapper.Map<CartDto>(cart);
    }

    public async Task AddItemAsync(string userId, AddToCartDto dto)
    {
        // ── Biznes məntiq: Məhsul və ya kolleksiya seçilməlidir ──────────
        if (dto.ProductId is null && dto.CollectionId is null)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "item", new List<string> { ValidationMessages.Get(lang, "ProductOrCollectionRequired") } }
                });

        // ── Biznes məntiq: Miqdar 1-dən az ola bilməz ───────────────────
        if (dto.Quantity < 1)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "quantity", new List<string> { ValidationMessages.Get(lang, "GreaterThanZero", "Miqdar") } }
                });

        // ── Biznes məntiq: Məhsulun mövcudluğu və stoku yoxlanır ─────────
        if (dto.ProductId.HasValue)
        {
            var product = await _productReadRepo.GetByIdAsync(dto.ProductId.Value);

            if (product is null || product.IsDeleted)
                throw new NotFoundException(ValidationMessages.Get(lang, "ProductNotFound"));

            // Stok = 0 → satış olmaz
            if (product.Stock <= 0)
                throw new Application.Exceptions.ValidationException(
                    new Dictionary<string, List<string>>
                    {
                        { "stock", new List<string> { ValidationMessages.Get(lang, "ProductOutOfStock", product.Translations.FirstOrDefault()?.Name ?? "Məhsul") } }
                    });

            // Seçilən miqdar mövcud stokdan çox ola bilməz
            if (dto.Quantity > product.Stock)
                throw new Application.Exceptions.ValidationException(
                    new Dictionary<string, List<string>>
                    {
                        { "stock", new List<string> { ValidationMessages.Get(lang, "CartQuantityExceedsStock", product.Stock) } }
                    });
        }

        // ── Biznes məntiq: Kolleksiyanın mövcudluğu yoxlanır ────────────
        if (dto.CollectionId.HasValue)
        {
            var collection = await _collectionReadRepo.GetByIdAsync(dto.CollectionId.Value);
            if (collection is null || collection.IsDeleted)
                throw new NotFoundException(ValidationMessages.Get(lang, "CollectionNotFound"));
        }

        var cart = await _readRepo.GetByUserIdAsync(userId, lang);
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            await _writeRepo.AddAsync(cart);
            await _writeRepo.SaveChangesAsync();
        }

        var existing = cart.Items.FirstOrDefault(x =>
            x.ProductId    == dto.ProductId    &&
            x.CollectionId == dto.CollectionId &&
            x.SelectedColor == dto.SelectedColor &&
            x.SelectedSize  == dto.SelectedSize);

        if (existing is not null)
        {
            // ── Biznes məntiq: Toplam miqdar stoku keçməsin ──────────────
            if (dto.ProductId.HasValue)
            {
                var product = await _productReadRepo.GetByIdAsync(dto.ProductId.Value);
                var newQty  = existing.Quantity + dto.Quantity;
                if (product is not null && newQty > product.Stock)
                    throw new Application.Exceptions.ValidationException(
                        new Dictionary<string, List<string>>
                        {
                            { "stock", new List<string> { ValidationMessages.Get(lang, "CartQuantityExceedsStock", product.Stock) } }
                        });
            }

            existing.Quantity += dto.Quantity;
            _writeRepo.Update(cart);
            _log.Information("Səbətdəki məhsulun miqdarı artırıldı — UserId: {UserId} ProductId: {ProductId} YeniMiqdar: {Quantity}",
                userId, dto.ProductId, existing.Quantity);
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId        = cart.Id,
                ProductId     = dto.ProductId,
                CollectionId  = dto.CollectionId,
                SelectedColor = dto.SelectedColor,
                SelectedSize  = dto.SelectedSize,
                Quantity      = dto.Quantity
            });
            _writeRepo.Update(cart);
            _log.Information("Səbətə məhsul əlavə edildi — UserId: {UserId} ProductId: {ProductId} CollectionId: {CollectionId}",
                userId, dto.ProductId, dto.CollectionId);
        }

        await _writeRepo.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(string userId, int cartItemId, int quantity)
    {
        var cart = await _readRepo.GetByUserIdAsync(userId, lang);
        if (cart is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CartNotFound"));

        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId);
        if (item is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CartItemNotFound"));

        if (quantity <= 0)
        {
            cart.Items.Remove(item);
            _log.Information("Səbətdən məhsul silindi (miqdar 0) — UserId: {UserId} CartItemId: {CartItemId}", userId, cartItemId);
        }
        else
        {
            // ── Biznes məntiq: Yeni miqdar stoku keçməsin ───────────────
            if (item.ProductId.HasValue)
            {
                var product = await _productReadRepo.GetByIdAsync(item.ProductId.Value);

                if (product is null || product.IsDeleted)
                    throw new Application.Exceptions.ValidationException(
                        new Dictionary<string, List<string>>
                        {
                            { "product", new List<string> { ValidationMessages.Get(lang, "CartProductUnavailable", "Məhsul") } }
                        });

                if (quantity > product.Stock)
                    throw new Application.Exceptions.ValidationException(
                        new Dictionary<string, List<string>>
                        {
                            { "stock", new List<string> { ValidationMessages.Get(lang, "CartQuantityExceedsStock", product.Stock) } }
                        });
            }

            _log.Information("Səbət miqdarı yeniləndi — UserId: {UserId} CartItemId: {CartItemId} YeniMiqdar: {New}",
                userId, cartItemId, quantity);
            item.Quantity = quantity;
        }

        _writeRepo.Update(cart);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(string userId, int cartItemId)
    {
        var cart = await _readRepo.GetByUserIdAsync(userId, lang);
        if (cart is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CartNotFound"));

        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId);
        if (item is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CartItemNotFound"));

        cart.Items.Remove(item);
        _writeRepo.Update(cart);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Səbətdən məhsul silindi — UserId: {UserId} CartItemId: {CartItemId}", userId, cartItemId);
    }

    public async Task ClearAsync(string userId)
    {
        var cart = await _readRepo.GetByUserIdAsync(userId, lang);
        if (cart is null) return;

        cart.Items.Clear();
        _writeRepo.Update(cart);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Səbət təmizləndi — UserId: {UserId}", userId);
    }
}
