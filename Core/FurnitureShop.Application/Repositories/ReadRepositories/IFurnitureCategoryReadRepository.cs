using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IFurnitureCategoryReadRepository : IGenericReadRepository<FurnitureCategory>
{
    Task<IEnumerable<FurnitureCategory>> GetAllWithTranslationsAsync(string lang);
    Task<FurnitureCategory?> GetWithProductsAsync(int id, string lang);
}
