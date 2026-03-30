namespace FurnitureShop.Application.Dtos.Admin;

public class TopProductDto
{
    public int     Id       { get; set; }
    public string  Name     { get; set; } = null!;
    public decimal Price    { get; set; }
    public int     Stock    { get; set; }
    public int     SoldCount{ get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
}
