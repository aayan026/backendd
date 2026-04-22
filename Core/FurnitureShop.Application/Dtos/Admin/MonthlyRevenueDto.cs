namespace FurnitureShop.Application.Dtos.Admin;

public class MonthlyRevenueDto
{
    public int     Month   { get; set; }  
    public int     Year    { get; set; }
    public decimal Revenue { get; set; }
    public int     Orders  { get; set; }
}
