namespace FurnitureShop.Domain.Entities.Concretes.Translation
{
    public class FurnitureCategoryTranslation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FurnitureCategory FurnitureCategory { get; set; }
        public int FurnitureCategoryId { get; set; }
        public string Lang { get; set; }
    }
}