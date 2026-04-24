using FurnitureShop.Domain.Entities.Common;

namespace FurnitureShop.Domain.Entities.Concretes;

public class ProductColorImage : BaseEntity
{
    public int ProductColorId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;

    public ProductColor ProductColor { get; set; } = null!;
}