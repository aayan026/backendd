namespace FurnitureShop.Application.Dtos.HeroSection;
public class HeroSectionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string? BadgeText { get; set; }
    public string? ImageUrl { get; set; }
}
