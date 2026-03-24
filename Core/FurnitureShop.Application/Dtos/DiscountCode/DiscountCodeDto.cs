using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Application.Dtos.DiscountCode;

public class DiscountCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public DiscountType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DiscountStatus Status { get; set; }
}
