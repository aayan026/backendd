using AutoMapper;
using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Product;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Mapping;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;

namespace FurnitureShop.Persistence.Services.Concretes;

public class ProductService : IProductService
{
    private readonly IProductReadRepository _readRepo;
    private readonly ICollectionReadRepository _collectionReadRepo;
    private readonly IProductWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;

    private string Lang => _langService.GetCurrentLanguage();

    public ProductService(
        IProductReadRepository readRepo,
        ICollectionReadRepository collectionReadRepo,
        IProductWriteRepository writeRepo,
        ILanguageService langService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _collectionReadRepo = collectionReadRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
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
        => _mapper.Map<IEnumerable<ProductDto>>(await _readRepo.GetFeaturedAsync(Lang));

    public async Task<ProductDto?> GetDetailAsync(int id)
    {
        var product = await _readRepo.GetDetailAsync(id, Lang);
        if (product is null) throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));
        return _mapper.Map<ProductDto>(product);
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
        return product.Id;
    }

    public async Task UpdateAsync(UpdateProductDto dto)
    {
        var product = await _readRepo.GetDetailAsync(dto.Id, Lang);
        if (product is null) throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));

        _mapper.Map(dto, product);
        _writeRepo.Update(product);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _readRepo.GetByIdAsync(id);
        if (product is null) throw new NotFoundException(ValidationMessages.Get(Lang, "ProductNotFound"));
        _writeRepo.Delete(product);
        await _writeRepo.SaveChangesAsync();
    }
}
