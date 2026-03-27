namespace FurnitureShop.Application.Dtos.Admin;

public class AdminDashboardDto
{
    public OrderStatsDto Orders { get; set; } = new();
    public decimal Revenue { get; set; }
    public int UserCount { get; set; }
}
