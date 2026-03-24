using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes.Translation
{
    public class CollectionCategoryTranslation
    {
        public int Id { get; set; }
        public int CollectionCategoryId { get; set; }
        public CollectionCategory CollectionCategory { get; set; }
        public string Name { get; set; }
        public string Lang { get; set; }
    }
}
