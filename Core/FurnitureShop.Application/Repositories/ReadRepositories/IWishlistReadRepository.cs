using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IWishlistReadRepository : IGenericReadRepository<Wishlist>
{
    Task<Wishlist?> GetByUserIdAsync(string userId, string lang);
}
