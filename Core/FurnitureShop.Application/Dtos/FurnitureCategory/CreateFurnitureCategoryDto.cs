namespace FurnitureShop.Application.Dtos.FurnitureCategory;
public class CreateFurnitureCategoryDto
{
    public string? ImageUrl { get; set; }
    public List<FurnitureCategoryTranslationDto> Translations { get; set; } = new();
}
