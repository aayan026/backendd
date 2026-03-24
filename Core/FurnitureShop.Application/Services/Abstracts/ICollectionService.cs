using FurnitureShop.Application.Dtos.Collection;

namespace FurnitureShop.Application.Services.Abstracts;

public interface ICollectionService
{
    Task<IEnumerable<CollectionDto>> GetAllAsync();
    Task<IEnumerable<CollectionDto>> GetByCategoryAsync(int categoryId);
    Task<CollectionDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateCollectionDto dto);
    Task UpdateAsync(UpdateCollectionDto dto);
    Task DeleteAsync(int id);
}
