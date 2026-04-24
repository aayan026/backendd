namespace FurnitureShop.Application.Dtos.Product;

public class ProductColorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string HexCode { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public List<ProductColorImageDto> Images { get; set; } = new();
}

public class ProductColorImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}