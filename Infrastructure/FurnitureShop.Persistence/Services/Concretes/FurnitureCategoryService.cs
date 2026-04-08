using AutoMapper;
using FurnitureShop.Application.Dtos.FurnitureCategory;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;

namespace FurnitureShop.Persistence.Services.Concretes;

public class FurnitureCategoryService : IFurnitureCategoryService
{
    private readonly IFurnitureCategoryReadRepository _readRepo;
    private readonly IFurnitureCategoryWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;

    private string Lang => _langService.GetCurrentLanguage();

    public FurnitureCategoryService(
        IFurnitureCategoryReadRepository readRepo,
        IFurnitureCategoryWriteRepository writeRepo,
        ILanguageService langService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FurnitureCategoryDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<FurnitureCategoryDto>>(
            await _readRepo.GetAllWithTranslationsAsync(Lang));

    public async Task<FurnitureCategoryDto?> GetByIdAsync(int id)
    {
        var category = await _readRepo.GetWithProductsAsync(id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CategoryNotFound"));
        return _mapper.Map<FurnitureCategoryDto>(category);
    }

    public async Task<int> CreateAsync(CreateFurnitureCategoryDto dto)
    {
        var category = _mapper.Map<FurnitureCategory>(dto);
        category.Translations = dto.Translations
            .Select(t => new FurnitureCategoryTranslation { Lang = t.Lang, Name = t.Name })
            .ToList();
        await _writeRepo.AddAsync(category);
        await _writeRepo.SaveChangesAsync();
        return category.Id;
    }

    public async Task UpdateAsync(UpdateFurnitureCategoryDto dto)
    {
        var category = await _readRepo.GetWithProductsAsync(dto.Id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CategoryNotFound"));

        category.ImageUrl = dto.ImageUrl;

        // FIX: Clear()+Add() unique index conflict (FurnitureCategoryId+Lang) verir.
        // Mövcud entries-ləri update et, yenilərini əlavə et.
        foreach (var t in dto.Translations)
        {
            var existing = category.Translations.FirstOrDefault(x => x.Lang == t.Lang);
            if (existing != null)
            {
                existing.Name = t.Name;
            }
            else
            {
                category.Translations.Add(new FurnitureCategoryTranslation
                {
                    Lang = t.Lang,
                    Name = t.Name,
                    FurnitureCategoryId = dto.Id
                });
            }
        }

        _writeRepo.Update(category);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        // Məhsullarla birlikdə yüklə
        var category = await _readRepo.GetWithProductsAsync(id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CategoryNotFound"));

        // Bu kateqoriyaya aid bütün məhsulları soft-delete et
        // (ProductConfiguration-da OnDelete(Restrict) var — birbaşa silmək olmur)
        if (category.Products != null)
        {
            foreach (var product in category.Products)
                product.IsDeleted = true;
        }

        _writeRepo.Delete(category);
        await _writeRepo.SaveChangesAsync();
    }
}