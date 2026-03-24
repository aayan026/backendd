using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface ICollectionReadRepository : IGenericReadRepository<Collection>
{
    Task<IEnumerable<Collection>> GetByCategoryAsync(int categoryId, string lang);
    Task<Collection?> GetWithProductsAsync(int id, string lang);
    Task<IEnumerable<Collection>> GetAllWithTranslationsAsync(string lang);
}
