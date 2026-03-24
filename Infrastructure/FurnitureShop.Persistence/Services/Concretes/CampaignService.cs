using AutoMapper;
using FurnitureShop.Application.Dtos.Campaign;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;

namespace FurnitureShop.Persistence.Services.Concretes;

public class CampaignService : ICampaignService
{
    private readonly ICampaignReadRepository _readRepo;
    private readonly ICampaignWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;

    private string lang => _langService.GetCurrentLanguage();


    public CampaignService(ICampaignReadRepository readRepo, ICampaignWriteRepository writeRepo, ILanguageService langService, IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CampaignDto>> GetActiveAsync()
        => _mapper.Map<IEnumerable<CampaignDto>>(await _readRepo.GetActiveAsync(lang));

    public async Task<IEnumerable<CampaignDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<CampaignDto>>(await _readRepo.GetAllWithTranslationsAsync(lang));

    public async Task<int> CreateAsync(CreateCampaignDto dto)
    {
        var campaign = _mapper.Map<Campaign>(dto);
        campaign.Translations = dto.Translations.Select(t => new CampaignTranslation { Lang = t.Lang, Title = t.Title, Description = t.Description, ButtonText = t.ButtonText }).ToList();
        await _writeRepo.AddAsync(campaign);
        await _writeRepo.SaveChangesAsync();
        return campaign.Id;
    }

    public async Task DeleteAsync(int id)
    {
        var campaign = await _readRepo.GetByIdAsync(id);
        if (campaign is null) throw new NotFoundException(ValidationMessages.Get(lang, "CampaignNotFound"));
        _writeRepo.Delete(campaign);
        await _writeRepo.SaveChangesAsync();
    }
}
