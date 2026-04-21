using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface ICartReadRepository : IGenericReadRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(string userId, string lang);
}
