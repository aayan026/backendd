namespace FurnitureShop.Application.Dtos.Product;
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;        // aktiv dilə görə
    public string? Description { get; set; }          // aktiv dilə görə
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal? PriceExtra { get; set; }
    public string? Label { get; set; }
    public string? Material { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public int Stock { get; set; }
    public int FurnitureCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductColorDto> Colors { get; set; } = new();
}
