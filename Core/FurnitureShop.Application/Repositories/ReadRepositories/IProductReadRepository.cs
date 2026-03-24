using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IProductReadRepository : IGenericReadRepository<Product>
{
    Task<IEnumerable<Product>> GetAllAsync(string lang);

    // Kateqoriyaya görə məhsullar (şəkil + translation ilə)
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, string lang);

    // Öne çıxarılmış məhsullar
    Task<IEnumerable<Product>> GetFeaturedAsync(string lang);

    // Tam detalı ilə bir məhsul (şəkil + rəng + translation)
    Task<Product?> GetDetailAsync(int id, string lang);

    // Axtarış (ada və təsvirə görə, dilə görə)
    Task<IEnumerable<Product>> SearchAsync(string keyword, string lang);

    // Qiymət aralığına görə
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal min, decimal max, string lang);

    // Rəngə görə filter
    Task<IEnumerable<Product>> GetByColorAsync(string colorName, string lang);

    // Stokda olanlar
    Task<IEnumerable<Product>> GetInStockAsync(string lang);
}
