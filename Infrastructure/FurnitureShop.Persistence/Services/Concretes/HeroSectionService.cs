using AutoMapper;
using FurnitureShop.Application.Dtos.HeroSection;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class HeroSectionService : IHeroSectionService
{
    private readonly IHeroSectionReadRepository  _readRepo;
    private readonly IHeroSectionWriteRepository _writeRepo;
    private readonly ILanguageService            _langService;
    private readonly IMapper                     _mapper;
    private static readonly ILogger _log = Log.ForContext<HeroSectionService>();

    private string lang => _langService.GetCurrentLanguage();

    public HeroSectionService(
        IHeroSectionReadRepository  readRepo,
        IHeroSectionWriteRepository writeRepo,
        ILanguageService            langService,
        IMapper                     mapper)
    {
        _readRepo    = readRepo;
        _writeRepo   = writeRepo;
        _langService = langService;
        _mapper      = mapper;
    }

    public async Task<IEnumerable<HeroSectionDto>> GetActiveAsync()
    {
        _log.Information("Aktiv hero section-lar sorğusu");
        return _mapper.Map<IEnumerable<HeroSectionDto>>(await _readRepo.GetActiveAsync(lang));
    }

    public async Task<IEnumerable<HeroSectionDto>> GetAllAsync()
    {
        _log.Information("Bütün hero section-lar sorğusu");
        return _mapper.Map<IEnumerable<HeroSectionDto>>(
            await _readRepo.GetAll().Include(h => h.Translations).ToListAsync());
    }

    public async Task<int> CreateAsync(CreateHeroSectionDto dto)
    {
        _log.Information("Yeni hero section yaradılır");
        var hero = _mapper.Map<HeroSection>(dto);
        hero.IsActive = true;
        hero.Translations = dto.Translations.Select(t => new HeroTranslation
        {
            Lang = t.Lang, Title = t.Title, Subtitle = t.Subtitle, BadgeText = t.BadgeText
        }).ToList();
        await _writeRepo.AddAsync(hero);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Hero section yaradıldı — HeroId: {HeroId}", hero.Id);
        return hero.Id;
    }

    public async Task UpdateAsync(int id, CreateHeroSectionDto dto)
    {
        _log.Information("Hero section yenilənir — HeroId: {HeroId}", id);
        var hero = await _readRepo.GetAll()
            .Include(h => h.Translations)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hero is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "HeroNotFound"));

        hero.ImageUrl = dto.ImageUrl ?? hero.ImageUrl;
        foreach (var t in dto.Translations)
        {
            var existing = hero.Translations.FirstOrDefault(x => x.Lang == t.Lang);
            if (existing is not null)
            {
                existing.Title = t.Title; existing.Subtitle = t.Subtitle; existing.BadgeText = t.BadgeText;
            }
            else
            {
                hero.Translations.Add(new HeroTranslation { Lang = t.Lang, Title = t.Title, Subtitle = t.Subtitle, BadgeText = t.BadgeText });
            }
        }

        _writeRepo.Update(hero);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Hero section yeniləndi — HeroId: {HeroId}", id);
    }

    public async Task ToggleAsync(int id)
    {
        var hero = await _readRepo.GetByIdAsync(id);
        if (hero is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "HeroNotFound"));

        hero.IsActive = !hero.IsActive;
        _writeRepo.Update(hero);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Hero section statusu dəyişdirildi — HeroId: {HeroId} IsActive: {IsActive}", id, hero.IsActive);
    }

    public async Task DeleteAsync(int id)
    {
        _log.Information("Hero section silinir — HeroId: {HeroId}", id);
        var hero = await _readRepo.GetByIdAsync(id);
        if (hero is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "HeroNotFound"));

        _writeRepo.Delete(hero);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Hero section silindi — HeroId: {HeroId}", id);
    }
}
