using FurnitureShop.Application.Dtos.HeroSection;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IHeroSectionService
{
    Task<IEnumerable<HeroSectionDto>> GetActiveAsync();
    Task<IEnumerable<HeroSectionDto>> GetAllAsync();
    Task<int> CreateAsync(CreateHeroSectionDto dto);
    Task UpdateAsync(int id, CreateHeroSectionDto dto);
    Task ToggleAsync(int id);
    Task DeleteAsync(int id);
}