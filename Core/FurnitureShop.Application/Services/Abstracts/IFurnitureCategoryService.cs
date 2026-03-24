using FurnitureShop.Application.Dtos.FurnitureCategory;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IFurnitureCategoryService
{
    Task<IEnumerable<FurnitureCategoryDto>> GetAllAsync();
    Task<FurnitureCategoryDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateFurnitureCategoryDto dto);
    Task UpdateAsync(UpdateFurnitureCategoryDto dto);
    Task DeleteAsync(int id);
}
