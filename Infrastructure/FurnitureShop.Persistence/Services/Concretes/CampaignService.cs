using System.Text.Json;
using AutoMapper;
using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Campaign;
using FurnitureShop.Application.Dtos.Product;
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

public class CampaignService : ICampaignService
{
    private readonly ICampaignReadRepository _readRepo;
    private readonly ICampaignWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;
    private static readonly ILogger _log = Log.ForContext<CampaignService>();

    private string Lang => _langService.GetCurrentLanguage();

    public CampaignService(
        ICampaignReadRepository readRepo,
        ICampaignWriteRepository writeRepo,
        ILanguageService langService,
        IMapper mapper,
        AppDbContext db)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
        _db = db;
    }

    public async Task<IEnumerable<CampaignDto>> GetActiveAsync()
    {
        var campaigns = await _readRepo.GetActiveAsync(Lang);
        return campaigns.Select(MapWithScope).ToList();
    }

    public async Task<PagedList<CampaignDto>> GetAllAsync(PaginationParams pagination)
    {
        var query = _readRepo.GetAllQuery(Lang);
        var paged = await PagedList<Campaign>.CreateAsync(query, pagination.Page, pagination.PageSize);
        return new PagedList<CampaignDto>
        {
            Items = paged.Items.Select(MapWithScope).ToList(),
            Pagination = paged.Pagination
        };
    }

    public async Task<PagedList<ProductDto>> GetCampaignProductsAsync(int campaignId, PaginationParams pagination)
    {
        var campaign = await _readRepo.GetAll()
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.IsActive);

        if (campaign is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CampaignNotFound"));

        IQueryable<Product> query;

        switch (campaign.ScopeType)
        {
            case CampaignScopeType.Products:
                {
                    var ids = ParseIds(campaign.ProductIds);
                    query = _db.Products.Where(p => !p.IsDeleted && ids.Contains(p.Id));
                    break;
                }
            case CampaignScopeType.Collections:
                {
                    var ids = ParseIds(campaign.CollectionIds);
                    var productIds = await _db.Collections
                        .Where(c => !c.IsDeleted && ids.Contains(c.Id))
                        .SelectMany(c => c.Products)
                        .Where(p => !p.IsDeleted)
                        .Select(p => p.Id)
                        .Distinct()
                        .ToListAsync();
                    query = _db.Products.Where(p => !p.IsDeleted && productIds.Contains(p.Id));
                    break;
                }
            case CampaignScopeType.Categories:
                {
                    var ids = ParseIds(campaign.CategoryIds);
                    query = _db.Products.Where(p => !p.IsDeleted && ids.Contains(p.FurnitureCategoryId));
                    break;
                }
            default:
                query = _db.Products.Where(p => !p.IsDeleted);
                break;
        }

        query = query
            .Include(p => p.Translations.Where(t => t.Lang == Lang))
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Include(p => p.Colors)
            .OrderBy(p => p.DisplayOrder);

        var paged = await PagedList<Product>.CreateAsync(query, pagination.Page, pagination.PageSize);
        return new PagedList<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged.Items),
            Pagination = paged.Pagination
        };
    }

    public async Task<int> CreateAsync(CreateCampaignDto dto)
    {
        _log.Information("Yeni kampaniya yaradılır — ScopeType: {Scope}", dto.ScopeType);
        Validate(dto);

        var campaign = new Campaign
        {
            ImageUrl = dto.ImageUrl,
            ButtonLink = dto.ButtonLink,
            DiscountPercent = dto.DiscountPercent,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true,
            ScopeType = dto.ScopeType,
            ProductIds = SerializeIds(dto.ProductIds),
            CollectionIds = SerializeIds(dto.CollectionIds),
            CategoryIds = SerializeIds(dto.CategoryIds),
            Translations = dto.Translations.Select(t => new CampaignTranslation
            {
                Lang = t.Lang,
                Title = t.Title,
                Description = t.Description,
                ButtonText = t.ButtonText
            }).ToList()
        };

        await _writeRepo.AddAsync(campaign);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kampaniya yaradıldı — CampaignId: {Id}", campaign.Id);
        return campaign.Id;
    }

    public async Task UpdateAsync(int id, CreateCampaignDto dto)
    {
        _log.Information("Kampaniya yenilənir — CampaignId: {Id}", id);

        var campaign = await _readRepo.GetAll()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "CampaignNotFound"));

        Validate(dto, existing: campaign);

        campaign.ImageUrl = dto.ImageUrl ?? campaign.ImageUrl;
        campaign.ButtonLink = dto.ButtonLink ?? campaign.ButtonLink;
        campaign.DiscountPercent = dto.DiscountPercent ?? campaign.DiscountPercent;
        campaign.StartDate = dto.StartDate != default ? dto.StartDate : campaign.StartDate;
        campaign.EndDate = dto.EndDate != default ? dto.EndDate : campaign.EndDate;
        campaign.DisplayOrder = dto.DisplayOrder;
        campaign.ScopeType = dto.ScopeType;
        campaign.ProductIds = SerializeIds(dto.ProductIds);
        campaign.CollectionIds = SerializeIds(dto.CollectionIds);
        campaign.CategoryIds = SerializeIds(dto.CategoryIds);

        foreach (var t in dto.Translations)
        {
            var ex = campaign.Translations.FirstOrDefault(x => x.Lang == t.Lang);
            if (ex is not null) { ex.Title = t.Title; ex.Description = t.Description; ex.ButtonText = t.ButtonText; }
            else campaign.Translations.Add(new CampaignTranslation { Lang = t.Lang, Title = t.Title, Description = t.Description, ButtonText = t.ButtonText });
        }

        _writeRepo.Update(campaign);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kampaniya yeniləndi — CampaignId: {Id}", id);
    }

    public async Task ToggleAsync(int id)
    {
        var campaign = await _readRepo.GetByIdAsync(id);
        if (campaign is null) throw new NotFoundException(ValidationMessages.Get(Lang, "CampaignNotFound"));
        campaign.IsActive = !campaign.IsActive;
        _writeRepo.Update(campaign);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var campaign = await _readRepo.GetByIdAsync(id);
        if (campaign is null) throw new NotFoundException(ValidationMessages.Get(Lang, "CampaignNotFound"));
        _writeRepo.Delete(campaign);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Kampaniya silindi — CampaignId: {Id}", id);
    }


    private CampaignDto MapWithScope(Campaign c)
    {
        var dto = _mapper.Map<CampaignDto>(c);
        dto.ScopeType = c.ScopeType;
        dto.ProductIds = ParseIds(c.ProductIds);
        dto.CollectionIds = ParseIds(c.CollectionIds);
        dto.CategoryIds = ParseIds(c.CategoryIds);
        return dto;
    }

    private static List<int> ParseIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<List<int>>(json) ?? new(); }
        catch { return new(); }
    }

    private static string? SerializeIds(List<int>? ids)
        => ids is null || ids.Count == 0 ? null : JsonSerializer.Serialize(ids);

    private void Validate(CreateCampaignDto dto, Campaign? existing = null)
    {
        var requiredLangs = new[] { "az", "ru", "en" };
        if (!requiredLangs.All(l => dto.Translations.Any(t => t.Lang == l)))
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "translations", new List<string> { ValidationMessages.Get(Lang, "AllLangsRequired") } } });

        if (dto.DiscountPercent.HasValue && (dto.DiscountPercent < 1 || dto.DiscountPercent > 100))
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "discountPercent", new List<string> { ValidationMessages.Get(Lang, "DiscountCodeInvalidValue") } } });

        var start = dto.StartDate != default ? dto.StartDate : existing?.StartDate ?? default;
        var end = dto.EndDate != default ? dto.EndDate : existing?.EndDate ?? default;
        if (start != default && end != default && end <= start)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "endDate", new List<string> { ValidationMessages.Get(Lang, "CampaignDateInvalid") } } });

        if (dto.ScopeType == CampaignScopeType.Products && (dto.ProductIds is null || dto.ProductIds.Count == 0))
            throw new Application.Exceptions.ValidationException(new Dictionary<string, List<string>> { { "productIds", new List<string> { "Ən az 1 məhsul seçilməlidir." } } });
        if (dto.ScopeType == CampaignScopeType.Collections && (dto.CollectionIds is null || dto.CollectionIds.Count == 0))
            throw new Application.Exceptions.ValidationException(new Dictionary<string, List<string>> { { "collectionIds", new List<string> { "Ən az 1 kolleksiya seçilməlidir." } } });
        if (dto.ScopeType == CampaignScopeType.Categories && (dto.CategoryIds is null || dto.CategoryIds.Count == 0))
            throw new Application.Exceptions.ValidationException(new Dictionary<string, List<string>> { { "categoryIds", new List<string> { "Ən az 1 kateqoriya seçilməlidir." } } });
    }
}
