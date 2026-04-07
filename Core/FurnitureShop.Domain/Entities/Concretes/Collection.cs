using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;

public class Collection:BaseEntity
{
    public ICollection<CollectionTranslation> Translations { get; set; }

    public string? ImagesUrl { get; set; }
    public CollectionCategory? CollectionCategory { get; set; }
    public int CollectionCategoryId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public bool IsDeleted { get; set; } = false;

    public int DisplayOrder { get; set; }
    public ICollection<Product> Products { get; set; }
}