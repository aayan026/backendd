namespace FurnitureShop.Application.Dtos.DiscountCode;

public class ValidateDiscountCodeDto
{
    public string  Code { get; set; } = null!;
    public decimal OrderTotal { get; set; }
}

