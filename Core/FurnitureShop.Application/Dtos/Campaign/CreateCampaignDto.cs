using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Dtos.Campaign;

public class CreateCampaignDto
{
    public string? ImageUrl { get; set; }
    public string? ButtonLink { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DisplayOrder { get; set; }

    public CampaignScopeType ScopeType { get; set; } = CampaignScopeType.All;
    public List<int> ProductIds { get; set; } = new();
    public List<int> CollectionIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();

    public List<CampaignTranslationDto> Translations { get; set; } = new();
}