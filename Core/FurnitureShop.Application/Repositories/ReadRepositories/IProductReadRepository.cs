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

    Task<Product?> GetByNameAsync(string name, string lang);

    Task<IEnumerable<Product>> GetSimilarAsync(int productId, int categoryId, decimal price, string? material, string lang);
}
