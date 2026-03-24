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

    private string lang => _langService.GetCurrentLanguage();


    public FurnitureCategoryService(IFurnitureCategoryReadRepository readRepo, IFurnitureCategoryWriteRepository writeRepo, ILanguageService langService, IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FurnitureCategoryDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<FurnitureCategoryDto>>(await _readRepo.GetAllWithTranslationsAsync(lang));

    public async Task<FurnitureCategoryDto?> GetByIdAsync(int id)
    {
        var category = await _readRepo.GetWithProductsAsync(id, lang);
        if (category is null) throw new NotFoundException(ValidationMessages.Get(lang, "CategoryNotFound"));
        return _mapper.Map<FurnitureCategoryDto>(category);
    }

    public async Task<int> CreateAsync(CreateFurnitureCategoryDto dto)
    {
        var category = _mapper.Map<FurnitureCategory>(dto);
        category.Translations = dto.Translations.Select(t => new FurnitureCategoryTranslation { Lang = t.Lang, Name = t.Name }).ToList();
        await _writeRepo.AddAsync(category);
        await _writeRepo.SaveChangesAsync();
        return category.Id;
    }

    public async Task UpdateAsync(UpdateFurnitureCategoryDto dto)
    {
        var category = await _readRepo.GetByIdAsync(dto.Id);
        if (category is null) throw new NotFoundException(ValidationMessages.Get(lang, "CategoryNotFound"));
        _mapper.Map(dto, category);
        _writeRepo.Update(category);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _readRepo.GetByIdAsync(id);
        if (category is null) throw new NotFoundException(ValidationMessages.Get(lang, "CategoryNotFound"));
        _writeRepo.Delete(category);
        await _writeRepo.SaveChangesAsync();
    }
}
