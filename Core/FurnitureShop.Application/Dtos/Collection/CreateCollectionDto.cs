namespace FurnitureShop.Application.Dtos.Collection;
public class CreateCollectionDto
{
    public string? ImageUrl { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int DisplayOrder { get; set; }
    public int CollectionCategoryId { get; set; }
    public List<int> ProductIds { get; set; } = new();
    public List<CollectionTranslationDto> Translations { get; set; } = new();
}
