using FurnitureShop.Domain.Entities.Common;

namespace FurnitureShop.Domain.Entities.Concretes.Translation
{
    public class CampaignTranslation
    {
        public int Id { get; set; }

        public int CampaignId { get; set; }      
        public string? Lang { get; set; }      
        public string? Title { get; set; }
        public string? Description { get; set; }

        public string? ButtonText { get; set; }
        public Campaign Campaign { get; set; }   


    }
}