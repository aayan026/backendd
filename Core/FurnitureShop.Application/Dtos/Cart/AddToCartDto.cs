namespace FurnitureShop.Application.Dtos.Cart;
public class AddToCartDto
{
    public int? ProductId { get; set; }
    public int? CollectionId { get; set; }
    public string? SelectedColor { get; set; }
    public string? SelectedSize { get; set; }
    public int Quantity { get; set; } = 1;
}
