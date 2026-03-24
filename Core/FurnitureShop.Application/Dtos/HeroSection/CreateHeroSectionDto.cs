namespace FurnitureShop.Application.Dtos.HeroSection;
public class CreateHeroSectionDto
{
    public string? ImageUrl { get; set; }
    public List<HeroTranslationDto> Translations { get; set; } = new();
}
