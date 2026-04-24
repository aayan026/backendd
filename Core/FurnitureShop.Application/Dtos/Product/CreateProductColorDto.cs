namespace FurnitureShop.Application.Dtos.Product;

public class CreateProductColorDto
{
    public string Name { get; set; } = null!;
    public string HexCode { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public List<CreateProductColorImageDto> Images { get; set; } = new();
}

public class CreateProductColorImageDto
{
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}