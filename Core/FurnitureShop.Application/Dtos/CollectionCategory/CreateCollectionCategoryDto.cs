namespace FurnitureShop.Application.Dtos.CollectionCategory;
public class CreateCollectionCategoryDto
{
    public string? ImageUrl { get; set; }
    public List<CollectionCategoryTranslationDto> Translations { get; set; } = new();
}
