using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface ICampaignReadRepository : IGenericReadRepository<Campaign>
{
    Task<IEnumerable<Campaign>> GetActiveAsync(string lang);
    Task<IEnumerable<Campaign>> GetAllWithTranslationsAsync(string lang);
}
