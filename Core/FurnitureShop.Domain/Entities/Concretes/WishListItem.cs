using FurnitureShop.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;


public class WishlistItem :BaseEntity
{
    public int WishlistId { get; set; }

    public int? ProductId { get; set; }
    public int? CollectionId { get; set; }

    public Wishlist Wishlist { get; set; } = null!;
    public Product? Product { get; set; }
    public Collection? Collection { get; set; }
}
