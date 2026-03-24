using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;

public class FurnitureCategory:BaseEntity
{
    public ICollection<FurnitureCategoryTranslation> Translations { get; set; }
    [JsonIgnore]
    public ICollection<Product> Products { get; set; }

}
