using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class CollectionReadRepository : GenericReadRepository<Collection>, ICollectionReadRepository
{
    public CollectionReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Collection>> GetByCategoryAsync(int categoryId, string lang)
        => await Table
            .Where(x => x.CollectionCategoryId == categoryId && !x.IsDeleted)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    public async Task<Collection?> GetWithProductsAsync(int id, string lang)
        => await Table
            .Where(x => x.Id == id)
            // FIX: update üçün bütün dillər yüklənir
            .Include(x => x.Translations)
            .Include(x => x.CollectionCategory)
                .ThenInclude(c => c!.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Products.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Products)
                .ThenInclude(p => p.Images.Where(i => i.IsPrimary))
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<Collection>> GetAllWithTranslationsAsync(string lang)
        => await Table
            .Where(x => !x.IsDeleted)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
}
