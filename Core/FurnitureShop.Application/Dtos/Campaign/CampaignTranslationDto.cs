namespace FurnitureShop.Application.Dtos.Campaign;
public class CampaignTranslationDto
{
    public string Lang { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ButtonText { get; set; }
}
