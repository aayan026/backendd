namespace FurnitureShop.Application.Dtos.Product;
public class UpdateProductDto
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal? PriceExtra { get; set; }
    public string? Label { get; set; }
    public string? Material { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public int Stock { get; set; }
    public int FurnitureCategoryId { get; set; }
    public List<CreateProductColorDto> Colors    { get; set; } = new();
    public List<ProductImageDto>       ImageUrls { get; set; } = new();
    public List<ProductTranslationDto> Translations { get; set; } = new();

    public decimal? Width  { get; set; }
    public decimal? Height { get; set; }
    public decimal? Depth  { get; set; }
    public decimal? Weight { get; set; }
}
