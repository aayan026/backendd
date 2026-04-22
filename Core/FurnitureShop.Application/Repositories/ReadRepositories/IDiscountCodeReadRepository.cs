using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IDiscountCodeReadRepository : IGenericReadRepository<DiscountCode>
{
    Task<DiscountCode?> GetByCodeAsync(string code);

    Task<IEnumerable<DiscountCode>> GetActiveAsync();
    Task<IEnumerable<DiscountCode>> GetAllAsync();

}
