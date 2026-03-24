namespace FurnitureShop.Domain.Entities.Concretes.Translation
{
    public class CollectionTranslation
    {
        public int Id { get; set; }
        public int CollectionId { get; set; }
        public string Lang { get; set; }        // "az", "en", "ru"
        public string Name { get; set; }
        public string Description { get; set; }

        public Collection Collection { get; set; }
    }
}