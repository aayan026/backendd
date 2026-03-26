namespace FurnitureShop.Application.Dtos.DiscountCode;

public class ValidateDiscountCodeDto
{
    public string  Code       { get; set; } = null!;
    public decimal OrderTotal { get; set; }
}

public class DiscountCodeValidationResult
{
    public bool     IsValid        { get; set; }
    public string?  Message        { get; set; }
    public int?     DiscountCodeId { get; set; }
    public decimal  DiscountAmount { get; set; }
    public decimal  FinalTotal     { get; set; }
}
