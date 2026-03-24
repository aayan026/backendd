namespace FurnitureShop.Application.Dtos.Cart;
public class CartDto
{
    public int Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal GrandTotal => Items.Sum(x => x.TotalPrice);
    public int ItemCount => Items.Sum(x => x.Quantity);
}
