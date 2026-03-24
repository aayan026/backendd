using FurnitureShop.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;


public class CartItem:BaseEntity
{
    public int CartId { get; set; }
    
    public int? ProductId { get; set; }
    public int? CollectionId { get; set; }

    public int Quantity { get; set; } = 1;
    public string? SelectedColor { get; set; }
    public string? SelectedSize { get; set; }

    public Cart Cart { get; set; } = null!;
    public Product? Product { get; set; }
    public Collection? Collection { get; set; }
}

