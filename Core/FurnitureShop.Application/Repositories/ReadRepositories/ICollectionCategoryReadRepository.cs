using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface ICollectionCategoryReadRepository : IGenericReadRepository<CollectionCategory>
{
    Task<IEnumerable<CollectionCategory>> GetAllWithTranslationsAsync(string lang);
    Task<CollectionCategory?> GetWithCollectionsAsync(int id, string lang);
}
