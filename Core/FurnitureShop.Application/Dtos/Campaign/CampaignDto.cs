using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Dtos.Campaign;

public class CampaignDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    public CampaignScopeType ScopeType { get; set; }
    public List<int> ProductIds { get; set; } = new();
    public List<int> CollectionIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}