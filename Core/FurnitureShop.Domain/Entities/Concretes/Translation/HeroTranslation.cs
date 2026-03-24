namespace FurnitureShop.Domain.Entities.Concretes.Translation
{
    public class HeroTranslation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string BadgeText { get; set; }
        public string Lang { get; set; }

        public int HeroId { get; set; }
        public HeroSection HeroSection { get; set; }
    }
}