using FurnitureShop.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;

public class OrderItem:BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public int? ProductId { get; set; }       
    public Product Product { get; set; }
    public int? CollectionId { get; set; }     
    public Collection Collection { get; set; }
    public string SelectedColor { get; set; }
    public string SelectedSize { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string CustomDescription { get; set; } 
}
