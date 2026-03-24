using FurnitureShop.Application.Dtos.CollectionCategory;

namespace FurnitureShop.Application.Services.Abstracts;

public interface ICollectionCategoryService
{
    Task<IEnumerable<CollectionCategoryDto>> GetAllAsync();
    Task<CollectionCategoryDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateCollectionCategoryDto dto);
    Task UpdateAsync(UpdateCollectionCategoryDto dto);
    Task DeleteAsync(int id);
}
