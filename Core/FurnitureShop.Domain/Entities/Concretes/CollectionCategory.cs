using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Concretes.Translation;

namespace FurnitureShop.Domain.Entities.Concretes;

public class CollectionCategory : BaseEntity
{
    public string ImageUrl { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public int collectionCount { get; set; }
    public ICollection<CollectionCategoryTranslation> Translations { get; set; } = new List<CollectionCategoryTranslation>();
    public ICollection<Collection> Collections { get; set; } = new List<Collection>();
}
