using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IHeroSectionReadRepository : IGenericReadRepository<HeroSection>
{
    Task<IEnumerable<HeroSection>> GetActiveAsync(string lang);
}
