using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IWishlistReadRepository : IGenericReadRepository<Wishlist>
{
    // İstifadəçinin wishlist-i — items + product ilə
    Task<Wishlist?> GetByUserIdAsync(string userId);
}
