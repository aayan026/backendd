namespace FurnitureShop.Application.Dtos.Admin;

public class OrderStatsDto
{
    public int Total      { get; set; }
    public int Pending    { get; set; }
    public int Confirmed  { get; set; }
    public int InProgress { get; set; }
    public int Delivered  { get; set; }
    public int Cancelled  { get; set; }
}
