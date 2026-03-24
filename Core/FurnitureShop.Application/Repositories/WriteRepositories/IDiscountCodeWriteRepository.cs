using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.WriteRepositories;

public interface IDiscountCodeWriteRepository : IGenericWriteRepository<DiscountCode>
{
    Task RemoveAsync(DiscountCode entity);
}
