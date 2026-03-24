namespace FurnitureShop.Application.Dtos.CollectionCategory;
public class UpdateCollectionCategoryDto
{
    public int Id { get; set; }
    public string? ImageUrl { get; set; }
    public List<CollectionCategoryTranslationDto> Translations { get; set; } = new();
}
