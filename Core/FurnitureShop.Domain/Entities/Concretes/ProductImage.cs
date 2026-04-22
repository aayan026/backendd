using FurnitureShop.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;


public class ProductImage:BaseEntity
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public Product Product { get; set; } = null!;
}
