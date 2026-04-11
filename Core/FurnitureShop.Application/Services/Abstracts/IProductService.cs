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

    /// <summary>Ada (slug) görə məhsul — frontend /products/:slug üçün</summary>
    Task<ProductDto?>                GetByNameAsync(string name);

    Task<PagedList<ProductDto>>      GetPagedAsync(int categoryId, PaginationParams pagination);
    Task<PagedList<ProductDto>>      SearchAsync(string keyword, PaginationParams pagination);
    Task<PagedList<ProductDto>>      GetByPriceRangeAsync(decimal min, decimal max, PaginationParams pagination);
    Task<PagedList<ProductDto>>      GetByColorAsync(string colorName, PaginationParams pagination);

    /// <summary>Bütün məhsullarda mövcud olan unikal rənglər (filter üçün)</summary>
    Task<IEnumerable<ProductColorDto>> GetDistinctColorsAsync();

    /// <summary>Oxşar məhsullar: eyni kateqoriya + yaxın qiymət + material</summary>
    Task<IEnumerable<ProductDto>>    GetSimilarAsync(int productId);

    Task<int>  CreateAsync(CreateProductDto dto);
    Task       UpdateAsync(UpdateProductDto dto);
    Task       DeleteAsync(int id);
}
