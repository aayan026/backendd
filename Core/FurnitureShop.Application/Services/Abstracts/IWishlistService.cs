using FurnitureShop.Application.Dtos.Wishlist;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IWishlistService
{
    Task<WishlistDto> GetAsync(string userId);
    Task AddItemAsync(string userId, int? productId, int? collectionId);
    Task RemoveItemAsync(string userId, int wishlistItemId);
    Task<bool> IsInWishlistAsync(string userId, int? productId, int? collectionId);
}
