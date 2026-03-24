using FurnitureShop.Application.Dtos.HeroSection;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IHeroSectionService
{
    Task<IEnumerable<HeroSectionDto>> GetActiveAsync();
    Task<IEnumerable<HeroSectionDto>> GetAllAsync();
    Task<int> CreateAsync(CreateHeroSectionDto dto);
    Task DeleteAsync(int id);
}
