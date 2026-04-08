using AutoMapper;
using FurnitureShop.Application.Dtos.Collection;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using FurnitureShop.Persistence.Datas;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Services.Concretes;

public class CollectionService : ICollectionService
{
    private readonly ICollectionReadRepository _readRepo;
    private readonly ICollectionWriteRepository _writeRepo;
    private readonly IProductReadRepository _productReadRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;

    private string Lang => _langService.GetCurrentLanguage();

    public CollectionService(
        ICollectionReadRepository readRepo,
        ICollectionWriteRepository writeRepo,
        IProductReadRepository productReadRepo,
        ILanguageService langService,
        IMapper mapper,
        AppDbContext db)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _productReadRepo = productReadRepo;
        _langService = langService;
        _mapper = mapper;
        _db = db;
    }

    public async Task<IEnumerable<CollectionDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<CollectionDto>>(
            await _readRepo.GetAllWithTranslationsAsync(Lang));

    public async Task<IEnumerable<CollectionDto>> GetByCategoryAsync(int categoryId)
        => _mapper.Map<IEnumerable<CollectionDto>>(
            await _readRepo.GetByCategoryAsync(categoryId, Lang));

    public async Task<CollectionDto?> GetByIdAsync(int id)
    {
        var collection = await _readRepo.GetWithProductsAsync(id, Lang);
        if (collection is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionNotFound"));
        return _mapper.Map<CollectionDto>(collection);
    }

    public async Task<int> CreateAsync(CreateCollectionDto dto)
    {
        var collection = _mapper.Map<Collection>(dto);
        collection.Translations = dto.Translations
            .Select(t => new CollectionTranslation
            {
                Lang = t.Lang,
                Name = t.Name,
                Description = t.Description
            }).ToList();

        // Məhsulları əlaqələndir
        if (dto.ProductIds.Any())
        {
            var products = new List<Product>();
            foreach (var pid in dto.ProductIds)
            {
                var p = await _productReadRepo.GetByIdAsync(pid);
                if (p != null) products.Add(p);
            }
            collection.Products = products;
        }

        await _writeRepo.AddAsync(collection);
        await _writeRepo.SaveChangesAsync();
        return collection.Id;
    }

    public async Task UpdateAsync(UpdateCollectionDto dto)
    {
        // FIX: translations ilə birlikdə yüklə
        var collection = await _readRepo.GetWithProductsAsync(dto.Id, Lang);
        if (collection is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionNotFound"));

        collection.ImagesUrl = dto.ImageUrl;
        collection.TotalPrice = dto.TotalPrice;
        collection.DiscountPrice = dto.DiscountPrice;
        collection.DisplayOrder = dto.DisplayOrder;
        collection.CollectionCategoryId = dto.CollectionCategoryId;

        // FIX: köhnə translations-ları birbaşa DB-dən sil, yenilərini əlavə et
        await _db.CollectionTranslations
            .Where(t => t.CollectionId == dto.Id)
            .ExecuteDeleteAsync();
        await _db.CollectionTranslations.AddRangeAsync(
            dto.Translations.Select(t => new CollectionTranslation
            {
                CollectionId = dto.Id,
                Lang = t.Lang,
                Name = t.Name,
                Description = t.Description
            }));

        // Məhsulları güncəllə
        collection.Products.Clear();
        if (dto.ProductIds.Any())
        {
            foreach (var pid in dto.ProductIds)
            {
                var p = await _productReadRepo.GetByIdAsync(pid);
                if (p != null) collection.Products.Add(p);
            }
        }

        _writeRepo.Update(collection);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var collection = await _readRepo.GetByIdAsync(id);
        if (collection is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionNotFound"));
        _writeRepo.Delete(collection);
        await _writeRepo.SaveChangesAsync();
    }
}
