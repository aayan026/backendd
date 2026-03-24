using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class HeroSectionReadRepository : GenericReadRepository<HeroSection>, IHeroSectionReadRepository
{
    public HeroSectionReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<HeroSection>> GetActiveAsync(string lang)
        => await Table
            .Where(x => x.IsActive)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .ToListAsync();
}
