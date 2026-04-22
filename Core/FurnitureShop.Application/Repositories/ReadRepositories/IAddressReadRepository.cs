using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IAddressReadRepository : IGenericReadRepository<Address>
{
    Task<IEnumerable<Address>> GetByUserIdAsync(string userId);

    Task<Address?> GetDefaultAddressAsync(string userId);
}
