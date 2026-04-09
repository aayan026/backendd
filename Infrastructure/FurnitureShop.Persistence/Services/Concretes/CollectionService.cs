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
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class CollectionService : ICollectionService
{
    private readonly ICollectionReadRepository  _readRepo;
    private readonly ICollectionWriteRepository _writeRepo;
    private readonly IProductReadRepository     _productReadRepo;
    private readonly ILanguageService           _langService;
    private readonly IMapper                    _mapper;
    private readonly AppDbContext               _db;
    private static readonly ILogger _log = Log.ForContext<CollectionService>();

    private string Lang => _langService.GetCurrentLanguage();

    public CollectionService(
        ICollectionReadRepository  readRepo,
        ICollectionWriteRepository writeRepo,
        IProductReadRepository     productReadRepo,
        ILanguageService           langService,
        IMapper                    mapper,
        AppDbContext               db)
    {
        _readRepo        = readRepo;
        _writeRepo       = writeRepo;
        _productReadRepo = productReadRepo;
        _langService     = langService;
        _mapper          = mapper;
        _db              = db;
    }

    public async Task<IEnumerable<CollectionDto>> GetAllAsync()
    {
        _log.Information("Bütün kolleksiyalar sorğusu");
        return _mapper.Map<IEnumerable<CollectionDto>>(await _readRepo.GetAllWithTranslationsAsync(Lang));
    }

    public async Task<IEnumerable<CollectionDto>> GetByCategoryAsync(int categoryId)
    {
        _log.Information("Kateqoriyaya görə kolleksiyalar — CategoryId: {CategoryId}", categoryId);
        return _mapper.Map<IEnumerable<CollectionDto>>(await _readRepo.GetByCategoryAsync(categoryId, Lang));
    }

    public async Task<CollectionDto?> GetByIdAsync(int id)
    {
        _log.Information("Kolleksiya detalı sorğusu — CollectionId: {CollectionId}", id);
        var collection = await _readRepo.GetWithProductsAsync(id, Lang);
        if (collection is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionNotFound"));
        return _mapper.Map<CollectionDto>(collection);
    }

    public async Task<int> CreateAsync(CreateCollectionDto dto)
    {
        _log.Information("Yeni kolleksiya yaradılır — MəhsulSayı: {ProductCount}", dto.ProductIds.Count);
        var collection = _mapper.Map<Collection>(dto);
        collection.Translations = dto.Translations
            .Select(t => new CollectionTranslation { Lang = t.Lang, Name = t.Name, Description = t.Description })
            .ToList();

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
        _log.Information("Kolleksiya yaradıldı — CollectionId: {CollectionId}", collection.Id);
        return collection.Id;
    }

    public async Task UpdateAsync(UpdateCollectionDto dto)
    {
        _log.Information("Kolleksiya yenilənir — CollectionId: {CollectionId}", dto.Id);
        var collection = await _readRepo.GetWithProductsAsync(dto.Id, Lang);
        if (collection is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionNotFound"));

        collection.ImagesUrl             = dto.ImageUrl;
        collection.TotalPrice            = dto.TotalPrice;
        collection.DiscountPrice         = dto.DiscountPrice;
        collection.DisplayOrder          = dto.DisplayOrder;
        collection.CollectionCategoryId  = dto.CollectionCategoryId;

        await _db.CollectionTranslations.Where(t => t.CollectionId == dto.Id).ExecuteDeleteAsync();
        await _db.CollectionTranslations.AddRangeAsync(
            dto.Translations.Select(t => new CollectionTranslation
            {
                CollectionId = dto.Id, Lang = t.Lang, Name = t.Name, Description = t.Description
            }));

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
        _log.Information("Kolleksiya yeniləndi — CollectionId: {CollectionId}", dto.Id);
    }

    public async Task DeleteAsync(int id)
    {
        _log.Information("Kolleksiya silinir — CollectionId: {CollectionId}", id);
        var collection = await _readRepo.GetByIdAsync(id);
        if (collection is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionNotFound"));
        _writeRepo.Delete(collection);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kolleksiya silindi — CollectionId: {CollectionId}", id);
    }
}
