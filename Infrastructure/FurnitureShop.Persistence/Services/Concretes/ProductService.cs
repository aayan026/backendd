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
using FurnitureShop.Domain.Entities.Enums;
using FurnitureShop.Persistence.Datas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class ProductService : IProductService
{
    private readonly IProductReadRepository _readRepo;
    private readonly ICollectionReadRepository _collectionReadRepo;
    private readonly IProductWriteRepository _writeRepo;
    private readonly IOrderReadRepository _orderReadRepo;
    private readonly ILanguageService _langService;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;
    private static readonly ILogger _log = Log.ForContext<ProductService>();

    private string Lang => _langService.GetCurrentLanguage();
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

    public ProductService(
        IProductReadRepository readRepo,
        ICollectionReadRepository collectionReadRepo,
        IProductWriteRepository writeRepo,
        IOrderReadRepository orderReadRepo,
        ILanguageService langService,
        IMemoryCache cache,
        IMapper mapper,
        AppDbContext db)
    {
        _readRepo = readRepo;
        _collectionReadRepo = collectionReadRepo;
        _writeRepo = writeRepo;
        _orderReadRepo = orderReadRepo;
        _langService = langService;
        _cache = cache;
        _mapper = mapper;
        _db = db;
    }

    public async Task<PagedList<ProductDto>> GetAllAsync(PaginationParams pagination)
    {
        _log.Information("Bütün məhsullar sorğusu — Səhifə: {Page} Ölçü: {PageSize}", pagination.Page, pagination.PageSize);
        var products = await _readRepo.GetAllAsync(Lang);
        var paged = PagedList<Product>.Create(products, pagination.Page, pagination.PageSize);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        _log.Information("Kateqoriyaya görə məhsullar — CategoryId: {CategoryId}", categoryId);
        return _mapper.Map<IEnumerable<ProductDto>>(await _readRepo.GetByCategoryAsync(categoryId, Lang));
    }

    public async Task<PagedList<ProductDto>> GetByCollectionAsync(int collectionId, PaginationParams pagination)
    {
        _log.Information("Kolleksiyaya görə məhsullar — CollectionId: {CollectionId}", collectionId);
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

            if (product.IsDeleted)
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
        _log.Information("Məhsul axtarışı — Açar söz: {Keyword}", keyword);
        var products = await _readRepo.SearchAsync(keyword, Lang);
        var paged = PagedList<Product>.Create(products, pagination.Page, pagination.PageSize);
        _log.Information("Axtarış nəticəsi — Açar söz: {Keyword} Tapılan: {Count}", keyword, paged.Pagination.TotalCount);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<PagedList<ProductDto>> GetByPriceRangeAsync(decimal min, decimal max, PaginationParams pagination)
    {
        if (min < 0 || max < 0)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "price", new List<string> { ValidationMessages.Get(Lang, "GreaterThanZero", "Qiymət") } } });

        if (min > max)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "price", new List<string> { "Minimum qiymət maksimumdan böyük ola bilməz" } } });

        _log.Information("Qiymət aralığına görə məhsullar — Min: {Min} Max: {Max}", min, max);
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
        var similar = await _readRepo.GetSimilarAsync(productId, product.FurnitureCategoryId, product.Price, product.Material, Lang);
        return _mapper.Map<IEnumerable<ProductDto>>(similar);
    }

    public async Task<int> CreateAsync(CreateProductDto dto)
    {
        _log.Information("Yeni məhsul yaradılır — Kateqoriya: {CategoryId} Qiymət: {Price}", dto.FurnitureCategoryId, dto.Price);

        if (dto.Price <= 0)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "price", new List<string> { ValidationMessages.Get(Lang, "GreaterThanZero", "Qiymət") } } });

        if (dto.Stock < 0)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "stock", new List<string> { "Stok mənfi ola bilməz" } } });

        var requiredLangs = new[] { "az", "ru", "en" };
        var providedLangs = dto.Translations.Select(t => t.Lang).ToHashSet();
        if (!requiredLangs.All(l => providedLangs.Contains(l)))
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "translations", new List<string> { ValidationMessages.Get(Lang, "AllLangsRequired") } } });

        if (dto.PriceExtra.HasValue && dto.PriceExtra >= dto.Price)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "priceExtra", new List<string> { ValidationMessages.Get(Lang, "DiscountLessThanTotal") } } });

        if (!dto.ImageUrls.Any())
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "images", new List<string> { ValidationMessages.Get(Lang, "MinCount", "Şəkillər", 1) } } });

        if (dto.ImageUrls.Count(i => i.IsPrimary) != 1)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "images", new List<string> { "Məhsulun tam olaraq 1 əsas şəkli olmalıdır" } } });

        var product = _mapper.Map<Product>(dto);
        product.Translations = dto.Translations.Select(t => _mapper.Map<ProductTranslation>(t)).ToList();
        product.Images = dto.ImageUrls.Select(i => new ProductImage { ImageUrl = i.ImageUrl, IsPrimary = i.IsPrimary }).ToList();
        product.Colors = dto.Colors.Select(c => {
            var color = _mapper.Map<ProductColor>(c);
            color.ColorImages = c.Images.Select((img, idx) => new ProductColorImage
            {
                ImageUrl = img.ImageUrl,
                IsPrimary = img.IsPrimary || idx == 0,
                SortOrder = img.SortOrder != 0 ? img.SortOrder : idx
            }).ToList();
            return color;
        }).ToList();

        await _writeRepo.AddAsync(product);
        await _writeRepo.SaveChangesAsync();

        _cache.Remove($"featured_products_{Lang}");
        _log.Information("Məhsul yaradıldı — ProductId: {ProductId}", product.Id);
        return product.Id;
    }

    public async Task UpdateAsync(UpdateProductDto dto)
    {
        _log.Information("Məhsul yenilənir — ProductId: {ProductId}", dto.Id);

        var product = await _readRepo.GetDetailAsync(dto.Id, "az");
        if (product is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));

        if (product.IsDeleted)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "product", new List<string> { ValidationMessages.Get(Lang, "ProductDeleted") } } });

        if (dto.Price <= 0)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "price", new List<string> { ValidationMessages.Get(Lang, "GreaterThanZero", "Qiymət") } } });
        if (dto.Stock < 0)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "stock", new List<string> { "Stok mənfi ola bilməz" } } });

        if (dto.PriceExtra.HasValue && dto.PriceExtra >= dto.Price)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "priceExtra", new List<string> { ValidationMessages.Get(Lang, "DiscountLessThanTotal") } } });

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

        await _db.ProductTranslations.Where(t => t.ProductId == dto.Id).ExecuteDeleteAsync();
        await _db.ProductTranslations.AddRangeAsync(
            dto.Translations.Select(t => new ProductTranslation
            {
                ProductId = dto.Id,
                Lang = t.Lang,
                Name = t.Name,
                Description = t.Description
            }));

        await _db.ProductImages.Where(i => i.ProductId == dto.Id).ExecuteDeleteAsync();
        await _db.ProductImages.AddRangeAsync(
            dto.ImageUrls.Select(i => new ProductImage { ProductId = dto.Id, ImageUrl = i.ImageUrl, IsPrimary = i.IsPrimary }));

        await _db.ProductColors.Where(c => c.ProductId == dto.Id).ExecuteDeleteAsync();
        var newColors = dto.Colors.Select(c => {
            var color = new ProductColor
            {
                ProductId = dto.Id,
                Name = c.Name,
                HexCode = c.HexCode,
                ImageUrl = c.ImageUrl
            };
            color.ColorImages = c.Images.Select((img, idx) => new ProductColorImage
            {
                ImageUrl = img.ImageUrl,
                IsPrimary = img.IsPrimary || idx == 0,
                SortOrder = img.SortOrder != 0 ? img.SortOrder : idx
            }).ToList();
            return color;
        }).ToList();
        await _db.ProductColors.AddRangeAsync(newColors);

        _writeRepo.Update(product);
        await _writeRepo.SaveChangesAsync();

        foreach (var lang in new[] { "az", "ru", "en" }) _cache.Remove($"product_{dto.Id}_{lang}");
        _cache.Remove($"featured_products_{Lang}");

        _log.Information("Məhsul yeniləndi — ProductId: {ProductId}", dto.Id);
    }

    public async Task<IEnumerable<ProductColorDto>> GetDistinctColorsAsync()
    {
        var colors = await _db.ProductColors
            .Where(c => !c.Product.IsDeleted)
            .Select(c => new ProductColorDto { Name = c.Name, HexCode = c.HexCode })
            .Distinct()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return colors
            .GroupBy(c => c.Name.Trim().ToLower())
            .Select(g => g.First())
            .ToList();
    }

    public async Task DeleteAsync(int id)
    {
        _log.Information("Məhsul silinir — ProductId: {ProductId}", id);

        var product = await _readRepo.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));

        if (product.IsDeleted)
            throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));

        var hasActiveOrders = await _db.OrderItems
            .AnyAsync(oi => oi.ProductId == id &&
                      oi.Order.Status != OrderStatus.Cancelled &&
                      oi.Order.Status != OrderStatus.Delivered);

        if (hasActiveOrders)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "product", new List<string> { ValidationMessages.Get(Lang, "ProductHasActiveOrders") } } });

        product.IsDeleted = true;
        _writeRepo.Update(product);
        await _writeRepo.SaveChangesAsync();

        foreach (var lang in new[] { "az", "ru", "en" }) _cache.Remove($"product_{id}_{lang}");
        _cache.Remove($"featured_products_{Lang}");

        _log.Information("Məhsul soft-delete edildi — ProductId: {ProductId}", id);
    }
}