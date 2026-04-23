using FurnitureShop.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;

public class ProductColor : BaseEntity
{
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string HexCode { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public Product Product { get; set; } = null!;
}