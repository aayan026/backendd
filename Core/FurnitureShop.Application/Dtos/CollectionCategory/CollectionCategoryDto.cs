namespace FurnitureShop.Application.Dtos.CollectionCategory;
public class CollectionCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal TotalPrice { get; set; }
}
