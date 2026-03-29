using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class FurnitureCategoryReadRepository : GenericReadRepository<FurnitureCategory>, IFurnitureCategoryReadRepository
{
    public FurnitureCategoryReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<FurnitureCategory>> GetAllWithTranslationsAsync(string lang)
        => await Table
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .ToListAsync();

    public async Task<FurnitureCategory?> GetWithProductsAsync(int id, string lang)
        => await Table
            .Where(x => x.Id == id)
            // FIX: bütün dillər yüklənir ki update zamanı clear + refill işləsin
            .Include(x => x.Translations)
            .Include(x => x.Products.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Products)
                .ThenInclude(p => p.Images.Where(i => i.IsPrimary))
            .FirstOrDefaultAsync();
}
