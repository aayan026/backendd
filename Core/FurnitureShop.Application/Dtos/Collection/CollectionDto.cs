using FurnitureShop.Application.Dtos.Product;
namespace FurnitureShop.Application.Dtos.Collection;
public class CollectionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int DisplayOrder { get; set; }
    public int CollectionCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<ProductDto> Products { get; set; } = new();
}
