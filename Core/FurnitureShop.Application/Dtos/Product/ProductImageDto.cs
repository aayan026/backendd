namespace FurnitureShop.Application.Dtos.Product;
public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
