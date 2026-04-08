using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IProductReadRepository : IGenericReadRepository<Product>
{
    Task<IEnumerable<Product>> GetAllAsync(string lang);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, string lang);
    Task<IEnumerable<Product>> GetFeaturedAsync(string lang);
    Task<Product?> GetDetailAsync(int id, string lang);
    Task<IEnumerable<Product>> SearchAsync(string keyword, string lang);
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal min, decimal max, string lang);
    Task<IEnumerable<Product>> GetByColorAsync(string colorName, string lang);
    Task<IEnumerable<Product>> GetInStockAsync(string lang);

    /// <summary>
    /// Ada görə məhsul tap — frontend slug (ad-based URL) ilə çağırır
    /// </summary>
    Task<Product?> GetByNameAsync(string name, string lang);

    /// <summary>
    /// Oxşar məhsullar: eyni kateqoriya + yaxın qiymət aralığı + (varsa) eyni material
    /// </summary>
    Task<IEnumerable<Product>> GetSimilarAsync(int productId, int categoryId, decimal price, string? material, string lang);

    /// <summary>
    /// Update üçün məhsulu BÜTÜn dillər ilə yüklə (lang filter olmadan).
    /// GetDetailAsync yalnız 1 dil yükləyir — .Clear() zamanı EF unique index conflict verir.
    /// </summary>
    Task<Product?> GetForUpdateAsync(int id);
}
