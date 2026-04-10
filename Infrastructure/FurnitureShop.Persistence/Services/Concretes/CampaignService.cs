using AutoMapper;
using FurnitureShop.Application.Dtos.Campaign;
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

public class CampaignService : ICampaignService
{
    private readonly ICampaignReadRepository  _readRepo;
    private readonly ICampaignWriteRepository _writeRepo;
    private readonly ILanguageService         _langService;
    private readonly IMapper                  _mapper;
    private static readonly ILogger _log = Log.ForContext<CampaignService>();

    private string lang => _langService.GetCurrentLanguage();

    public CampaignService(
        ICampaignReadRepository  readRepo,
        ICampaignWriteRepository writeRepo,
        ILanguageService         langService,
        IMapper                  mapper)
    {
        _readRepo    = readRepo;
        _writeRepo   = writeRepo;
        _langService = langService;
        _mapper      = mapper;
    }

    public async Task<IEnumerable<CampaignDto>> GetActiveAsync()
        => _mapper.Map<IEnumerable<CampaignDto>>(await _readRepo.GetActiveAsync(lang));

    public async Task<IEnumerable<CampaignDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<CampaignDto>>(await _readRepo.GetAllWithTranslationsAsync(lang));

    public async Task<int> CreateAsync(CreateCampaignDto dto)
    {
        _log.Information("Yeni kampaniya yaradılır");

        // ── Biznes məntiq: Az, ru, en dillərinin hamısı lazımdır ─────────
        var requiredLangs = new[] { "az", "ru", "en" };
        var providedLangs = dto.Translations.Select(t => t.Lang).ToHashSet();
        if (!requiredLangs.All(l => providedLangs.Contains(l)))
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "translations", new List<string> { ValidationMessages.Get(lang, "AllLangsRequired") } } });

        // ── Biznes məntiq: Endirim faizi 1-100 arasında olmalıdır ────────
        if (dto.DiscountPercent.HasValue && (dto.DiscountPercent < 1 || dto.DiscountPercent > 100))
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "discountPercent", new List<string> { ValidationMessages.Get(lang, "DiscountCodeInvalidValue") } } });

        // ── Biznes məntiq: Bitiş tarixi başlama tarixindən sonra olmalıdır ──
        if (dto.StartDate != default && dto.EndDate != default && dto.EndDate <= dto.StartDate)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "endDate", new List<string> { ValidationMessages.Get(lang, "CampaignDateInvalid") } } });

        // ── Biznes məntiq: Başlama tarixi gələcəkdə olmalıdır ────────────
        if (dto.StartDate != default && dto.StartDate < DateTime.UtcNow.Date)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "startDate", new List<string> { ValidationMessages.Get(lang, "FutureDate", "Başlama tarixi") } } });

        var campaign = _mapper.Map<Campaign>(dto);
        campaign.IsActive = true;
        campaign.Translations = dto.Translations.Select(t => new CampaignTranslation
        {
            Lang = t.Lang, Title = t.Title, Description = t.Description, ButtonText = t.ButtonText
        }).ToList();

        await _writeRepo.AddAsync(campaign);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kampaniya yaradıldı — CampaignId: {CampaignId}", campaign.Id);
        return campaign.Id;
    }

    public async Task UpdateAsync(int id, CreateCampaignDto dto)
    {
        _log.Information("Kampaniya yenilənir — CampaignId: {CampaignId}", id);

        var campaign = await _readRepo.GetAll()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CampaignNotFound"));

        // ── Biznes məntiq: Endirim faizi 1-100 arasında olmalıdır ────────
        if (dto.DiscountPercent.HasValue && (dto.DiscountPercent < 1 || dto.DiscountPercent > 100))
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "discountPercent", new List<string> { ValidationMessages.Get(lang, "DiscountCodeInvalidValue") } } });

        // ── Biznes məntiq: Bitiş tarixi başlama tarixindən sonra olmalıdır ──
        var newStart = dto.StartDate != default ? dto.StartDate : campaign.StartDate;
        var newEnd   = dto.EndDate   != default ? dto.EndDate   : campaign.EndDate;
        if (newStart != default && newEnd != default && newEnd <= newStart)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "endDate", new List<string> { ValidationMessages.Get(lang, "CampaignDateInvalid") } } });

        campaign.ImageUrl        = dto.ImageUrl        ?? campaign.ImageUrl;
        campaign.ButtonLink      = dto.ButtonLink      ?? campaign.ButtonLink;
        campaign.DiscountPercent = dto.DiscountPercent ?? campaign.DiscountPercent;
        campaign.StartDate       = newStart;
        campaign.EndDate         = newEnd;
        campaign.DisplayOrder    = dto.DisplayOrder;

        foreach (var t in dto.Translations)
        {
            var existing = campaign.Translations.FirstOrDefault(x => x.Lang == t.Lang);
            if (existing is not null)
            {
                existing.Title = t.Title; existing.Description = t.Description; existing.ButtonText = t.ButtonText;
            }
            else
            {
                campaign.Translations.Add(new CampaignTranslation
                {
                    Lang = t.Lang, Title = t.Title, Description = t.Description, ButtonText = t.ButtonText
                });
            }
        }

        _writeRepo.Update(campaign);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kampaniya yeniləndi — CampaignId: {CampaignId}", id);
    }

    public async Task ToggleAsync(int id)
    {
        var campaign = await _readRepo.GetByIdAsync(id);
        if (campaign is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CampaignNotFound"));

        campaign.IsActive = !campaign.IsActive;
        _writeRepo.Update(campaign);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kampaniya statusu dəyişdirildi — CampaignId: {CampaignId} IsActive: {IsActive}", id, campaign.IsActive);
    }

    public async Task DeleteAsync(int id)
    {
        var campaign = await _readRepo.GetByIdAsync(id);
        if (campaign is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "CampaignNotFound"));

        // ── Biznes məntiq: Aktiv kampaniya birbaşa silinə bilməz, əvvəl deaktiv edilməlidir ──
        if (campaign.IsActive)
        {
            campaign.IsActive = false;
            _writeRepo.Update(campaign);
        }

        _writeRepo.Delete(campaign);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kampaniya silindi — CampaignId: {CampaignId}", id);
    }
}
