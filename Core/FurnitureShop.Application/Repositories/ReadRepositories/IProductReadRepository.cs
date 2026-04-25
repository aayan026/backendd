using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IProductReadRepository : IGenericReadRepository<Product>
{
    IQueryable<Product> GetAllQuery(string lang);
    Task<IEnumerable<Product>> GetAllAsync(string lang);

    IQueryable<Product> GetByCategoryQuery(int categoryId, string lang);
    IQueryable<Product> SearchQuery(string keyword, string lang);
    IQueryable<Product> GetByPriceRangeQuery(decimal min, decimal max, string lang);
    IQueryable<Product> GetByColorQuery(string colorName, string lang);

    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, string lang);
    Task<IEnumerable<Product>> GetFeaturedAsync(string lang);
    Task<Product?> GetDetailAsync(int id, string lang);
    Task<IEnumerable<Product>> SearchAsync(string keyword, string lang);
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal min, decimal max, string lang);
    Task<IEnumerable<Product>> GetByColorAsync(string colorName, string lang);
    Task<IEnumerable<Product>> GetInStockAsync(string lang);
    Task<Product?> GetForUpdateAsync(int id);
    Task<Product?> GetByNameAsync(string name, string lang);
    Task<IEnumerable<Product>> GetSimilarAsync(int productId, int categoryId, decimal price, string? material, string lang);
}