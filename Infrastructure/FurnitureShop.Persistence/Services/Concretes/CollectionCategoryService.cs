using FurnitureShop.Application.Validation;
using AutoMapper;
using FurnitureShop.Application.Dtos.CollectionCategory;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class CollectionCategoryService : ICollectionCategoryService
{
    private readonly ICollectionCategoryReadRepository  _readRepo;
    private readonly ICollectionCategoryWriteRepository _writeRepo;
    private readonly ILanguageService                   _langService;
    private readonly IMapper                            _mapper;
    private static readonly ILogger _log = Log.ForContext<CollectionCategoryService>();

    private string Lang => _langService.GetCurrentLanguage();

    public CollectionCategoryService(
        ICollectionCategoryReadRepository  readRepo,
        ICollectionCategoryWriteRepository writeRepo,
        ILanguageService                   langService,
        IMapper                            mapper)
    {
        _readRepo    = readRepo;
        _writeRepo   = writeRepo;
        _langService = langService;
        _mapper      = mapper;
    }

    public async Task<IEnumerable<CollectionCategoryDto>> GetAllAsync()
    {
        _log.Information("Bütün kolleksiya kateqoriyaları sorğusu");
        return _mapper.Map<IEnumerable<CollectionCategoryDto>>(await _readRepo.GetAllWithTranslationsAsync(Lang));
    }

    public async Task<CollectionCategoryDto?> GetByIdAsync(int id)
    {
        _log.Information("Kolleksiya kateqoriyası sorğusu — CategoryId: {CategoryId}", id);
        var category = await _readRepo.GetWithCollectionsAsync(id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionCategoryNotFound"));
        return _mapper.Map<CollectionCategoryDto>(category);
    }

    public async Task<int> CreateAsync(CreateCollectionCategoryDto dto)
    {
        _log.Information("Yeni kolleksiya kateqoriyası yaradılır");
        var category = new CollectionCategory
        {
            ImageUrl     = dto.ImageUrl ?? null,
            Translations = dto.Translations
                .Select(t => new CollectionCategoryTranslation { Lang = t.Lang, Name = t.Name })
                .ToList()
        };
        await _writeRepo.AddAsync(category);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kolleksiya kateqoriyası yaradıldı — CategoryId: {CategoryId}", category.Id);
        return category.Id;
    }

    public async Task UpdateAsync(UpdateCollectionCategoryDto dto)
    {
        _log.Information("Kolleksiya kateqoriyası yenilənir — CategoryId: {CategoryId}", dto.Id);
        var category = await _readRepo.GetWithCollectionsAsync(dto.Id, "az");
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionCategoryNotFound"));

        category.ImageUrl = dto.ImageUrl ?? category.ImageUrl;
        category.Translations.Clear();
        foreach (var t in dto.Translations)
            category.Translations.Add(new CollectionCategoryTranslation
            {
                Lang = t.Lang, Name = t.Name, CollectionCategoryId = dto.Id
            });

        _writeRepo.Update(category);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kolleksiya kateqoriyası yeniləndi — CategoryId: {CategoryId}", dto.Id);
    }

    public async Task DeleteAsync(int id)
    {
        _log.Information("Kolleksiya kateqoriyası silinir — CategoryId: {CategoryId}", id);
        var category = await _readRepo.GetWithCollectionsAsync(id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionCategoryNotFound"));

        if (category.Collections != null)
        {
            foreach (var col in category.Collections)
                col.IsDeleted = true;
            _log.Information("Kateqoriyaya aid kolleksiyalar soft-delete edildi — CategoryId: {CategoryId} Sayı: {Count}", id, category.Collections.Count());
        }

        _writeRepo.Delete(category);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kolleksiya kateqoriyası silindi — CategoryId: {CategoryId}", id);
    }
}
