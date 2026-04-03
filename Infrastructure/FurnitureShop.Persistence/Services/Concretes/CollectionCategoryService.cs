using FurnitureShop.Application.Validation;
using AutoMapper;
using FurnitureShop.Application.Dtos.CollectionCategory;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;

namespace FurnitureShop.Persistence.Services.Concretes;

public class CollectionCategoryService : ICollectionCategoryService
{
    private readonly ICollectionCategoryReadRepository  _readRepo;
    private readonly ICollectionCategoryWriteRepository _writeRepo;
    private readonly ILanguageService                   _langService;
    private readonly IMapper                            _mapper;

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
        => _mapper.Map<IEnumerable<CollectionCategoryDto>>(
            await _readRepo.GetAllWithTranslationsAsync(Lang));

    public async Task<CollectionCategoryDto?> GetByIdAsync(int id)
    {
        var category = await _readRepo.GetWithCollectionsAsync(id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionCategoryNotFound"));
        return _mapper.Map<CollectionCategoryDto>(category);
    }

    public async Task<int> CreateAsync(CreateCollectionCategoryDto dto)
    {
        var category = new CollectionCategory
        {
            ImageUrl     = dto.ImageUrl ?? string.Empty,
            Translations = dto.Translations
                .Select(t => new CollectionCategoryTranslation { Lang = t.Lang, Name = t.Name })
                .ToList()
        };

        await _writeRepo.AddAsync(category);
        await _writeRepo.SaveChangesAsync();
        return category.Id;
    }

    public async Task UpdateAsync(UpdateCollectionCategoryDto dto)
    {
        // FIX: bütün dillər ilə yüklə ("az" deyil)
        var category = await _readRepo.GetWithCollectionsAsync(dto.Id, "az");
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionCategoryNotFound"));

        category.ImageUrl = dto.ImageUrl ?? category.ImageUrl;

        // FIX: köhnə translations-ları sil, yenilərini əlavə et
        category.Translations.Clear();
        foreach (var t in dto.Translations)
            category.Translations.Add(new CollectionCategoryTranslation
            {
                Lang                 = t.Lang,
                Name                 = t.Name,
                CollectionCategoryId = dto.Id
            });

        _writeRepo.Update(category);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        // Kolleksiyalarla birlikdə yüklə
        var category = await _readRepo.GetWithCollectionsAsync(id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CollectionCategoryNotFound"));

        // Bu kateqoriyaya aid kolleksiyaları soft-delete et
        // (CollectionConfiguration-da OnDelete(Restrict) var)
        if (category.Collections != null)
        {
            foreach (var col in category.Collections)
                col.IsDeleted = true;
        }

        _writeRepo.Delete(category);
        await _writeRepo.SaveChangesAsync();
    }
}
