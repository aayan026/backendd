namespace FurnitureShop.Application.Dtos.FurnitureCategory;
public class UpdateFurnitureCategoryDto
{
    public int Id { get; set; }
    public string? ImageUrl { get; set; }
    public List<FurnitureCategoryTranslationDto> Translations { get; set; } = new();
}
