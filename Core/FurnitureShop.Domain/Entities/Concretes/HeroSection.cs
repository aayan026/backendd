using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;

public class HeroSection:BaseEntity
{
    public ICollection<HeroTranslation> Translations { get; set; }
    public string ImageUrl { get; set; }
    public bool IsActive { get; set; }
}
