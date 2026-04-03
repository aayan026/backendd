using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;
public class Product:BaseEntity
{
    public decimal Price { get; set; }
    public int FurnitureCategoryId { get; set; }

    [JsonIgnore]
    public FurnitureCategory? FurnitureCategory { get; set; }
    public string Material { get; set; }
    public ICollection<ProductColor> Colors { get; set; } = new List<ProductColor>();
    public bool IsDeleted { get; set; } = false;
    public int Stock { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public string Label { get; set; }
    public decimal? PriceExtra { get; set; }
    public ICollection<ProductTranslation> Translations { get; set; }
    public decimal? Width  { get; set; }
    public decimal? Height { get; set; }
    public decimal? Depth  { get; set; }
    public decimal? Weight { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}