using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;
public class Campaign : BaseEntity
{

    public string ImageUrl { get; set; }

    public string ButtonLink { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public ICollection<CampaignTranslation> Translations { get; set; }
}
