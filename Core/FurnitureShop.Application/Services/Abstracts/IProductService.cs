using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Product;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IProductService
{
    Task<PagedList<ProductDto>>      GetAllAsync(PaginationParams pagination);
    Task<IEnumerable<ProductDto>>    GetByCategoryAsync(int categoryId);
    Task<PagedList<ProductDto>>      GetByCollectionAsync(int collectionId, PaginationParams pagination);
    Task<IEnumerable<ProductDto>>    GetFeaturedAsync();
    Task<ProductDto?>                GetDetailAsync(int id);

    Task<ProductDto?>                GetByNameAsync(string name);

    Task<PagedList<ProductDto>>      GetPagedAsync(int categoryId, PaginationParams pagination);
    Task<PagedList<ProductDto>>      SearchAsync(string keyword, PaginationParams pagination);
    Task<PagedList<ProductDto>>      GetByPriceRangeAsync(decimal min, decimal max, PaginationParams pagination);
    Task<PagedList<ProductDto>>      GetByColorAsync(string colorName, PaginationParams pagination);

    Task<IEnumerable<ProductColorDto>> GetDistinctColorsAsync();

    Task<IEnumerable<ProductDto>>    GetSimilarAsync(int productId);

    Task<int>  CreateAsync(CreateProductDto dto);
    Task       UpdateAsync(UpdateProductDto dto);
    Task       DeleteAsync(int id);
}
