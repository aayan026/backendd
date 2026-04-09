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
    private readonly ICartReadRepository  _readRepo;
    private readonly ICartWriteRepository _writeRepo;
    private readonly ILanguageService     _langService;
    private readonly IMapper              _mapper;
    private static readonly ILogger _log = Log.ForContext<CartService>();

    private string lang => _langService.GetCurrentLanguage();

    public CartService(
        ICartReadRepository  readRepo,
        ICartWriteRepository writeRepo,
        ILanguageService     langService,
        IMapper              mapper)
    {
        _readRepo    = readRepo;
        _writeRepo   = writeRepo;
        _langService = langService;
        _mapper      = mapper;
    }

    public async Task<CartDto> GetAsync(string userId)
    {
        _log.Information("Səbət sorğusu — UserId: {UserId}", userId);
        var cart = await _readRepo.GetByUserIdAsync(userId);
        if (cart is null)
            return new CartDto();
        return _mapper.Map<CartDto>(cart);
    }

    public async Task AddItemAsync(string userId, AddToCartDto dto)
    {
        if (dto.ProductId is null && dto.CollectionId is null)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "item", new List<string> { ValidationMessages.Get(lang, "ProductOrCollectionRequired") } }
                });

        var cart = await _readRepo.GetByUserIdAsync(userId);

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
            existing.Quantity += dto.Quantity;
            _writeRepo.Update(cart);
            _log.Information("Səbətdəki məhsulun miqdarı artırıldı — UserId: {UserId} ProductId: {ProductId} CollectionId: {CollectionId} YeniMiqdar: {Quantity}",
                userId, dto.ProductId, dto.CollectionId, existing.Quantity);
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
            _log.Information("Səbətə məhsul əlavə edildi — UserId: {UserId} ProductId: {ProductId} CollectionId: {CollectionId} Miqdar: {Quantity} Rəng: {Color} Ölçü: {Size}",
                userId, dto.ProductId, dto.CollectionId, dto.Quantity, dto.SelectedColor, dto.SelectedSize);
        }

        await _writeRepo.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(string userId, int cartItemId, int quantity)
    {
        var cart = await _readRepo.GetByUserIdAsync(userId);
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
            _log.Information("Səbət miqdarı yeniləndi — UserId: {UserId} CartItemId: {CartItemId} EskiMiqdar: {Old} YeniMiqdar: {New}",
                userId, cartItemId, item.Quantity, quantity);
            item.Quantity = quantity;
        }

        _writeRepo.Update(cart);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(string userId, int cartItemId)
    {
        var cart = await _readRepo.GetByUserIdAsync(userId);
        if (cart is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CartNotFound"));

        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId);
        if (item is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CartItemNotFound"));

        cart.Items.Remove(item);
        _writeRepo.Update(cart);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Səbətdən məhsul silindi — UserId: {UserId} CartItemId: {CartItemId} ProductId: {ProductId}",
            userId, cartItemId, item.ProductId);
    }

    public async Task ClearAsync(string userId)
    {
        var cart = await _readRepo.GetByUserIdAsync(userId);
        if (cart is null) return;

        var itemCount = cart.Items.Count;
        cart.Items.Clear();
        _writeRepo.Update(cart);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Səbət təmizləndi — UserId: {UserId} SilənMəhsulSayı: {Count}", userId, itemCount);
    }
}
