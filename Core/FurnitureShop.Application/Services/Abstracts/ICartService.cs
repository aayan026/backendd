using FurnitureShop.Application.Dtos.Cart;

namespace FurnitureShop.Application.Services.Abstracts;

public interface ICartService
{
    Task<CartDto> GetAsync(string userId);
    Task AddItemAsync(string userId, AddToCartDto dto);
    Task UpdateQuantityAsync(string userId, int cartItemId, int quantity);
    Task RemoveItemAsync(string userId, int cartItemId);
    Task ClearAsync(string userId);
}
