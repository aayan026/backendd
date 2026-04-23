using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Application.Dtos.Product;


public class CreateProductColorDto
{
    public string Name { get; set; } = null!;
    public string HexCode { get; set; } = null!;
    public string? ImageUrl { get; set; }
}