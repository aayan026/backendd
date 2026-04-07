using FurnitureShop.Domain.Entities.Common;

namespace FurnitureShop.Domain.Entities.Concretes.Translation;

public class ProductTranslation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? Lang { get; set; }    
    public string? Name { get; set; }
    public string? Description { get; set; }

    public Product Product { get; set; }
}