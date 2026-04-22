using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class ProductReadRepository : GenericReadRepository<Product>, IProductReadRepository
{
    public ProductReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetAllAsync(string lang)
        => await Table
            .Where(x => !x.IsDeleted)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images.Where(i => i.IsPrimary))
            .Include(x => x.Colors)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, string lang)
        => await Table
            .Where(x => x.FurnitureCategoryId == categoryId && !x.IsDeleted)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images)
            .Include(x => x.Colors)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    public async Task<IEnumerable<Product>> GetFeaturedAsync(string lang)
        => await Table
            .Where(x => x.IsFeatured && !x.IsDeleted)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images.Where(i => i.IsPrimary))
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    public async Task<Product?> GetDetailAsync(int id, string lang)
        => await Table
            .Where(x => x.Id == id && !x.IsDeleted)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images)
            .Include(x => x.Colors)
            .Include(x => x.FurnitureCategory)
                .ThenInclude(c => c.Translations.Where(t => t.Lang == lang))
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<Product>> SearchAsync(string keyword, string lang)
        => await Table
            .Where(x => !x.IsDeleted && x.Translations.Any(t =>
                t.Lang == lang &&
                (t.Name.Contains(keyword) || t.Description!.Contains(keyword))))
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images.Where(i => i.IsPrimary))
            .ToListAsync();

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal min, decimal max, string lang)
        => await Table
            .Where(x => !x.IsDeleted && x.Price >= min && x.Price <= max)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images.Where(i => i.IsPrimary))
            .ToListAsync();
    public async Task<Product?> GetForUpdateAsync(int id)
    {
        return await Table
            .Include(x => x.Translations)
            .Include(x => x.Images)
            .Include(x => x.Colors)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }
    public async Task<IEnumerable<Product>> GetByColorAsync(string colorName, string lang)
        => await Table
            .Where(x => !x.IsDeleted && x.Colors.Any(c => c.Name.Contains(colorName)))
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images.Where(i => i.IsPrimary))
            .Include(x => x.Colors)
            .ToListAsync();

    public Task<IEnumerable<Product>> GetInStockAsync(string lang)
        => Task.FromResult<IEnumerable<Product>>(
            Table
                .Where(x => !x.IsDeleted && x.Stock > 0)
                .Include(x => x.Translations.Where(t => t.Lang == lang))
                .Include(x => x.Images.Where(i => i.IsPrimary))
                .Include(x => x.Colors)
                .OrderBy(x => x.DisplayOrder)
                .AsEnumerable());

    public async Task<Product?> GetByNameAsync(string name, string lang)
    {
        var normalized = name.Replace("-", " ").ToLower();
        return await Table
            .Where(x => !x.IsDeleted &&
                        x.Translations.Any(t =>
                            t.Lang == lang &&
                            t.Name.ToLower() == normalized))
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images)
            .Include(x => x.Colors)
            .Include(x => x.FurnitureCategory)
                .ThenInclude(c => c.Translations.Where(t => t.Lang == lang))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetSimilarAsync(
        int productId, int categoryId, decimal price, string? material, string lang)
    {
        var minPrice = price * 0.5m;
        var maxPrice = price * 1.5m;

        var query = Table
            .Where(x => !x.IsDeleted && x.Id != productId && x.FurnitureCategoryId == categoryId)
            .Where(x => x.Price >= minPrice && x.Price <= maxPrice)
            .Include(x => x.Translations.Where(t => t.Lang == lang))
            .Include(x => x.Images.Where(i => i.IsPrimary))
            .Include(x => x.Colors)
            .OrderBy(x => x.DisplayOrder);

        var results = await query.ToListAsync();

        if (!string.IsNullOrWhiteSpace(material))
        {
            results = results
                .OrderByDescending(x => x.Material != null &&
                    x.Material.ToLower().Contains(material.ToLower()))
                .ThenBy(x => x.DisplayOrder)
                .ToList();
        }

        return results.Take(4);
    }
}
