using AutoMapper;
using FurnitureShop.Application.Dtos.Collection;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;

namespace FurnitureShop.Persistence.Services.Concretes;

public class CollectionService : ICollectionService
{
    private readonly ICollectionReadRepository _readRepo;
    private readonly ICollectionWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;

    private string lang => _langService.GetCurrentLanguage();


    public CollectionService(ICollectionReadRepository readRepo, ICollectionWriteRepository writeRepo, ILanguageService langService, IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CollectionDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<CollectionDto>>(await _readRepo.GetAllWithTranslationsAsync(lang));

    public async Task<IEnumerable<CollectionDto>> GetByCategoryAsync(int categoryId)
        => _mapper.Map<IEnumerable<CollectionDto>>(await _readRepo.GetByCategoryAsync(categoryId, lang));

    public async Task<CollectionDto?> GetByIdAsync(int id)
    {
        var collection = await _readRepo.GetWithProductsAsync(id, lang);
        if (collection is null) throw new NotFoundException(ValidationMessages.Get(lang, "CollectionNotFound"));
        return _mapper.Map<CollectionDto>(collection);
    }

    public async Task<int> CreateAsync(CreateCollectionDto dto)
    {
        var collection = _mapper.Map<Collection>(dto);
        collection.Translations = dto.Translations.Select(t => new CollectionTranslation { Lang = t.Lang, Name = t.Name, Description = t.Description }).ToList();
        await _writeRepo.AddAsync(collection);
        await _writeRepo.SaveChangesAsync();
        return collection.Id;
    }

    public async Task UpdateAsync(UpdateCollectionDto dto)
    {
        var collection = await _readRepo.GetByIdAsync(dto.Id);
        if (collection is null) throw new NotFoundException(ValidationMessages.Get(lang, "CollectionNotFound"));
        _mapper.Map(dto, collection);
        _writeRepo.Update(collection);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var collection = await _readRepo.GetByIdAsync(id);
        if (collection is null) throw new NotFoundException(ValidationMessages.Get(lang, "CollectionNotFound"));
        _writeRepo.Delete(collection);
        await _writeRepo.SaveChangesAsync();
    }
}
