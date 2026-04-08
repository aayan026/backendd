using AutoMapper;
using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Product;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using FurnitureShop.Persistence.Datas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureShop.Persistence.Services.Concretes;

public class ProductService : IProductService
{
    private readonly IProductReadRepository _readRepo;
    private readonly ICollectionReadRepository _collectionReadRepo;
    private readonly IProductWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;

    private string Lang => _langService.GetCurrentLanguage();
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

    public ProductService(
        IProductReadRepository readRepo,
        ICollectionReadRepository collectionReadRepo,
        IProductWriteRepository writeRepo,
        ILanguageService langService,
        IMemoryCache cache,
        IMapper mapper,
        AppDbContext db)
    {
        _readRepo = readRepo;
        _collectionReadRepo = collectionReadRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _cache = cache;
        _mapper = mapper;
        _db = db;
    }

    public async Task<PagedList<ProductDto>> GetAllAsync(PaginationParams pagination)
    {
        var products = await _readRepo.GetAllAsync(Lang);
        var paged = PagedList<Product>.Create(products, pagination.Page, pagination.PageSize);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId)
        => _mapper.Map<IEnumerable<ProductDto>>(await _readRepo.GetByCategoryAsync(categoryId, Lang));

    public async Task<PagedList<ProductDto>> GetByCollectionAsync(int collectionId, PaginationParams pagination)
    {
        var collection = await _collectionReadRepo.GetWithProductsAsync(collectionId, Lang);
        if (collection is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionNotFound"));

        var products = collection.Products.Where(x => !x.IsDeleted);
        var paged = PagedList<Product>.Create(products, pagination.Page, pagination.PageSize);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<IEnumerable<ProductDto>> GetFeaturedAsync()
    {
        var cacheKey = $"featured_products_{Lang}";
        if (!_cache.TryGetValue(cacheKey, out IEnumerable<ProductDto>? cached))
        {
            var products = await _readRepo.GetFeaturedAsync(Lang);
            cached = _mapper.Map<IEnumerable<ProductDto>>(products);
            _cache.Set(cacheKey, cached, CacheExpiry);
        }
        return cached!;
    }

    public async Task<ProductDto?> GetDetailAsync(int id)
    {
        var cacheKey = $"product_{id}_{Lang}";
        if (!_cache.TryGetValue(cacheKey, out ProductDto? cached))
        {
            var product = await _readRepo.GetDetailAsync(id, Lang);
            if (product is null)
                throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));
            cached = _mapper.Map<ProductDto>(product);
            _cache.Set(cacheKey, cached, CacheExpiry);
        }
        return cached;
    }

    public async Task<ProductDto?> GetByNameAsync(string name)
    {
        var cacheKey = $"product_name_{name}_{Lang}";
        if (!_cache.TryGetValue(cacheKey, out ProductDto? cached))
        {
            var product = await _readRepo.GetByNameAsync(name, Lang);
            if (product is null)
                throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));
            cached = _mapper.Map<ProductDto>(product);
            _cache.Set(cacheKey, cached, CacheExpiry);
        }
        return cached;
    }

    public async Task<PagedList<ProductDto>> GetPagedAsync(int categoryId, PaginationParams pagination)
    {
        var products = await _readRepo.GetByCategoryAsync(categoryId, Lang);
        var paged = PagedList<Product>.Create(products, pagination.Page, pagination.PageSize);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<PagedList<ProductDto>> SearchAsync(string keyword, PaginationParams pagination)
    {
        var products = await _readRepo.SearchAsync(keyword, Lang);
        var paged = PagedList<Product>.Create(products, pagination.Page, pagination.PageSize);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<PagedList<ProductDto>> GetByPriceRangeAsync(decimal min, decimal max, PaginationParams pagination)
    {
        var products = await _readRepo.GetByPriceRangeAsync(min, max, Lang);
        var paged = PagedList<Product>.Create(products, pagination.Page, pagination.PageSize);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<PagedList<ProductDto>> GetByColorAsync(string colorName, PaginationParams pagination)
    {
        var products = await _readRepo.GetByColorAsync(colorName, Lang);
        var paged = PagedList<Product>.Create(products, pagination.Page, pagination.PageSize);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<IEnumerable<ProductDto>> GetSimilarAsync(int productId)
    {
        var product = await _readRepo.GetDetailAsync(productId, Lang);
        if (product is null) return Enumerable.Empty<ProductDto>();

        var similar = await _readRepo.GetSimilarAsync(
            productId,
            product.FurnitureCategoryId,
            product.Price,
            product.Material,
            Lang);

        return _mapper.Map<IEnumerable<ProductDto>>(similar);
    }

    public async Task<int> CreateAsync(CreateProductDto dto)
    {
        var product = _mapper.Map<Product>(dto);

        product.Translations = dto.Translations
            .Select(t => _mapper.Map<ProductTranslation>(t))
            .ToList();

        product.Images = dto.ImageUrls
            .Select(i => new ProductImage { ImageUrl = i.ImageUrl, IsPrimary = i.IsPrimary })
            .ToList();

        product.Colors = dto.Colors
            .Select(c => _mapper.Map<ProductColor>(c))
            .ToList();

        await _writeRepo.AddAsync(product);
        await _writeRepo.SaveChangesAsync();

        _cache.Remove($"featured_products_{Lang}");
        return product.Id;
    }

    public async Task UpdateAsync(UpdateProductDto dto)
    {
        var product = await _readRepo.GetDetailAsync(dto.Id, "az");
        if (product is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));

        // Əsas sahələr
        product.Price = dto.Price;
        product.PriceExtra = dto.PriceExtra;
        product.Label = dto.Label;
        product.Material = dto.Material;
        product.IsFeatured = dto.IsFeatured;
        product.DisplayOrder = dto.DisplayOrder;
        product.Stock = dto.Stock;
        product.FurnitureCategoryId = dto.FurnitureCategoryId;
        product.Width = dto.Width;
        product.Height = dto.Height;
        product.Depth = dto.Depth;
        product.Weight = dto.Weight;

        // Translations — köhnəni birbaşa DB-dən sil, yenisini əlavə et
        await _db.ProductTranslations
            .Where(t => t.ProductId == dto.Id)
            .ExecuteDeleteAsync();
        await _db.ProductTranslations.AddRangeAsync(
            dto.Translations.Select(t => new ProductTranslation
            {
                ProductId = dto.Id,
                Lang = t.Lang,
                Name = t.Name,
                Description = t.Description
            }));

        // Images — köhnəni sil, yenisini əlavə et
        await _db.ProductImages
            .Where(i => i.ProductId == dto.Id)
            .ExecuteDeleteAsync();
        await _db.ProductImages.AddRangeAsync(
            dto.ImageUrls.Select(i => new ProductImage
            {
                ProductId = dto.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary
            }));

        // Colors — köhnəni sil, yenisini əlavə et
        await _db.ProductColors
            .Where(c => c.ProductId == dto.Id)
            .ExecuteDeleteAsync();
        await _db.ProductColors.AddRangeAsync(
            dto.Colors.Select(c => new ProductColor
            {
                ProductId = dto.Id,
                Name = c.Name,
                HexCode = c.HexCode
            }));

        _writeRepo.Update(product);
        await _writeRepo.SaveChangesAsync();

        _cache.Remove($"product_{dto.Id}_az");
        _cache.Remove($"product_{dto.Id}_ru");
        _cache.Remove($"product_{dto.Id}_en");
        _cache.Remove($"featured_products_{Lang}");
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _readRepo.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));

        _writeRepo.Delete(product);
        await _writeRepo.SaveChangesAsync();

        _cache.Remove($"product_{id}_az");
        _cache.Remove($"product_{id}_ru");
        _cache.Remove($"product_{id}_en");
        _cache.Remove($"featured_products_{Lang}");
    }
}
