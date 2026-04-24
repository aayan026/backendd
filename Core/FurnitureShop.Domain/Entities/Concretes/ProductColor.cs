using FurnitureShop.Domain.Entities.Common;

namespace FurnitureShop.Domain.Entities.Concretes;

public class ProductColor : BaseEntity
{
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string HexCode { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public Product Product { get; set; } = null!;

    public ICollection<ProductColorImage> ColorImages { get; set; } = new List<ProductColorImage>();
}