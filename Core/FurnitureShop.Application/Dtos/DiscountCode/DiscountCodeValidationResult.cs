using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Application.Dtos.DiscountCode;
public class DiscountCodeValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public int? DiscountCodeId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalTotal { get; set; }
}

