namespace FurnitureShop.Application.Dtos.Product;
public class ProductTranslationDto
{
    public string Lang { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
