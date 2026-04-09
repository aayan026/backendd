using AutoMapper;
using FurnitureShop.Application.Dtos.FurnitureCategory;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class FurnitureCategoryService : IFurnitureCategoryService
{
    private readonly IFurnitureCategoryReadRepository  _readRepo;
    private readonly IFurnitureCategoryWriteRepository _writeRepo;
    private readonly ILanguageService                  _langService;
    private readonly IMapper                           _mapper;
    private static readonly ILogger _log = Log.ForContext<FurnitureCategoryService>();

    private string Lang => _langService.GetCurrentLanguage();

    public FurnitureCategoryService(
        IFurnitureCategoryReadRepository  readRepo,
        IFurnitureCategoryWriteRepository writeRepo,
        ILanguageService                  langService,
        IMapper                           mapper)
    {
        _readRepo    = readRepo;
        _writeRepo   = writeRepo;
        _langService = langService;
        _mapper      = mapper;
    }

    public async Task<IEnumerable<FurnitureCategoryDto>> GetAllAsync()
    {
        _log.Information("Bütün mebel kateqoriyaları sorğusu");
        return _mapper.Map<IEnumerable<FurnitureCategoryDto>>(await _readRepo.GetAllWithTranslationsAsync(Lang));
    }

    public async Task<FurnitureCategoryDto?> GetByIdAsync(int id)
    {
        _log.Information("Mebel kateqoriyası sorğusu — CategoryId: {CategoryId}", id);
        var category = await _readRepo.GetWithProductsAsync(id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CategoryNotFound"));
        return _mapper.Map<FurnitureCategoryDto>(category);
    }

    public async Task<int> CreateAsync(CreateFurnitureCategoryDto dto)
    {
        _log.Information("Yeni mebel kateqoriyası yaradılır");
        var category = _mapper.Map<FurnitureCategory>(dto);
        category.Translations = dto.Translations
            .Select(t => new FurnitureCategoryTranslation { Lang = t.Lang, Name = t.Name })
            .ToList();
        await _writeRepo.AddAsync(category);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Mebel kateqoriyası yaradıldı — CategoryId: {CategoryId}", category.Id);
        return category.Id;
    }

    public async Task UpdateAsync(UpdateFurnitureCategoryDto dto)
    {
        _log.Information("Mebel kateqoriyası yenilənir — CategoryId: {CategoryId}", dto.Id);
        var category = await _readRepo.GetWithProductsAsync(dto.Id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CategoryNotFound"));

        category.ImageUrl = dto.ImageUrl;
        category.Translations.Clear();
        foreach (var t in dto.Translations)
            category.Translations.Add(new FurnitureCategoryTranslation
            {
                Lang = t.Lang, Name = t.Name, FurnitureCategoryId = dto.Id
            });

        _writeRepo.Update(category);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Mebel kateqoriyası yeniləndi — CategoryId: {CategoryId}", dto.Id);
    }

    public async Task DeleteAsync(int id)
    {
        _log.Information("Mebel kateqoriyası silinir — CategoryId: {CategoryId}", id);
        var category = await _readRepo.GetWithProductsAsync(id, Lang);
        if (category is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CategoryNotFound"));

        if (category.Products != null)
        {
            foreach (var product in category.Products)
                product.IsDeleted = true;
            _log.Information("Kateqoriyaya aid məhsullar soft-delete edildi — CategoryId: {CategoryId} Sayı: {Count}", id, category.Products.Count());
        }

        _writeRepo.Delete(category);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Mebel kateqoriyası silindi — CategoryId: {CategoryId}", id);
    }
}
