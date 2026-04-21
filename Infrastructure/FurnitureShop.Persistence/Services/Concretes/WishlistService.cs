using AutoMapper;
using FurnitureShop.Application.Dtos.Wishlist;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class WishlistService : IWishlistService
{
    private readonly IWishlistReadRepository   _readRepo;
    private readonly IWishlistWriteRepository  _writeRepo;
    private readonly IProductReadRepository    _productReadRepo;
    private readonly ICollectionReadRepository _collectionReadRepo;
    private readonly ILanguageService          _langService;
    private readonly IMapper                   _mapper;
    private static readonly ILogger _log = Log.ForContext<WishlistService>();

    private string lang => _langService.GetCurrentLanguage();

    public WishlistService(
        IWishlistReadRepository   readRepo,
        IWishlistWriteRepository  writeRepo,
        IProductReadRepository    productReadRepo,
        ICollectionReadRepository collectionReadRepo,
        ILanguageService          langService,
        IMapper                   mapper)
    {
        _readRepo           = readRepo;
        _writeRepo          = writeRepo;
        _productReadRepo    = productReadRepo;
        _collectionReadRepo = collectionReadRepo;
        _langService        = langService;
        _mapper             = mapper;
    }

    public async Task<WishlistDto> GetAsync(string userId)
    {
        _log.Information("İstək siyahısı sorğusu — UserId: {UserId}", userId);
        var wishlist = await _readRepo.GetByUserIdAsync(userId, lang);
        if (wishlist is null)
            return new WishlistDto();
        return _mapper.Map<WishlistDto>(wishlist);
    }

    public async Task AddItemAsync(string userId, int? productId, int? collectionId)
    {
        // ── Biznes məntiq: Məhsul və ya kolleksiya seçilməlidir ──────────
        if (productId is null && collectionId is null)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "item", new List<string> { ValidationMessages.Get(lang, "ProductOrCollectionRequired") } }
                });

        // ── Biznes məntiq: Məhsulun DB-də mövcudluğu yoxlanır ───────────
        if (productId.HasValue)
        {
            var product = await _productReadRepo.GetByIdAsync(productId.Value);
            if (product is null || product.IsDeleted)
                throw new NotFoundException(ValidationMessages.Get(lang, "WishlistProductNotFound"));
        }

        // ── Biznes məntiq: Kolleksiyanın DB-də mövcudluğu yoxlanır ──────
        if (collectionId.HasValue)
        {
            var collection = await _collectionReadRepo.GetByIdAsync(collectionId.Value);
            if (collection is null || collection.IsDeleted)
                throw new NotFoundException(ValidationMessages.Get(lang, "WishlistProductNotFound"));
        }

        var wishlist = await _readRepo.GetByUserIdAsync(userId, lang);
        if (wishlist is null)
        {
            wishlist = new Wishlist { UserId = userId };
            await _writeRepo.AddAsync(wishlist);
            await _writeRepo.SaveChangesAsync();
        }

        // ── Biznes məntiq: Eyni məhsul iki dəfə əlavə edilə bilməz ──────
        var alreadyExists = wishlist.Items.Any(x =>
            x.ProductId    == productId &&
            x.CollectionId == collectionId);

        if (alreadyExists)
        {
            _log.Information("Məhsul artıq istək siyahısındadır — UserId: {UserId} ProductId: {ProductId}", userId, productId);
            return; // Xəta atmırıq, sadəcə keçirik
        }

        wishlist.Items.Add(new WishlistItem
        {
            WishlistId   = wishlist.Id,
            ProductId    = productId,
            CollectionId = collectionId
        });

        _writeRepo.Update(wishlist);
        await _writeRepo.SaveChangesAsync();

        _log.Information("İstək siyahısına məhsul əlavə edildi — UserId: {UserId} ProductId: {ProductId}", userId, productId);
    }

    public async Task RemoveItemAsync(string userId, int wishlistItemId)
    {
        var wishlist = await _readRepo.GetByUserIdAsync(userId, lang);
        if (wishlist is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "WishlistNotFound"));

        var item = wishlist.Items.FirstOrDefault(x => x.Id == wishlistItemId);
        if (item is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "WishlistItemNotFound"));

        wishlist.Items.Remove(item);
        _writeRepo.Update(wishlist);
        await _writeRepo.SaveChangesAsync();

        _log.Information("İstək siyahısından məhsul silindi — UserId: {UserId} ItemId: {ItemId}", userId, wishlistItemId);
    }

    public async Task<bool> IsInWishlistAsync(string userId, int? productId, int? collectionId)
    {
        var wishlist = await _readRepo.GetByUserIdAsync(userId, lang);
        if (wishlist is null) return false;

        return wishlist.Items.Any(x =>
            x.ProductId    == productId &&
            x.CollectionId == collectionId);
    }
}
