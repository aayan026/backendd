using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;


public class Wishlist :BaseEntity
{    public string UserId { get; set; } = null!;
    
    public AppUser User { get; set; } = null!;
    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}
