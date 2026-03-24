namespace FurnitureShop.Application.Dtos.Order;
public class OrderItemDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImage { get; set; }
    public int? CollectionId { get; set; }
    public string? CollectionName { get; set; }
    public string? SelectedColor { get; set; }
    public string? SelectedSize { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
