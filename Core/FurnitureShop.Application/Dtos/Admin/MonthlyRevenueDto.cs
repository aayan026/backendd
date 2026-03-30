namespace FurnitureShop.Application.Dtos.Admin;

public class MonthlyRevenueDto
{
    public int     Month   { get; set; }   // 1–12
    public int     Year    { get; set; }
    public decimal Revenue { get; set; }
    public int     Orders  { get; set; }
}
