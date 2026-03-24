using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IAddressReadRepository : IGenericReadRepository<Address>
{
    // İstifadəçinin bütün ünvanları
    Task<IEnumerable<Address>> GetByUserIdAsync(string userId);

    // İstifadəçinin default ünvanı
    Task<Address?> GetDefaultAddressAsync(string userId);
}
