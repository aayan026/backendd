namespace FurnitureShop.Application.Dtos.HeroSection;
public class HeroTranslationDto
{
    public string Lang { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string? BadgeText { get; set; }
}
