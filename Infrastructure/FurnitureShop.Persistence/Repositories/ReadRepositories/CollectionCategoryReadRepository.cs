using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class CollectionCategoryReadRepository : GenericReadRepository<CollectionCategory>, ICollectionCategoryReadRepository
{
    public CollectionCategoryReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<CollectionCategory>> GetAllWithTranslationsAsync(string lang)
        => await Table
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .ToListAsync();


    public async Task<CollectionCategory?> GetWithCollectionsAsync(int id, string lang)
        => await Table
            .Where(x => x.Id == id)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Collections)
                .ThenInclude(c => c.Translations.Where(t => t.Lang == lang))
            .FirstOrDefaultAsync();
}
