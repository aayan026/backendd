using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class CampaignReadRepository : GenericReadRepository<Campaign>, ICampaignReadRepository
{
    public CampaignReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Campaign>> GetActiveAsync(string lang)
        => await Table
            .Where(x => x.IsActive && x.StartDate <= DateTime.UtcNow && x.EndDate >= DateTime.UtcNow)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    public async Task<IEnumerable<Campaign>> GetAllWithTranslationsAsync(string lang)
        => await Table
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .ToListAsync();


}
