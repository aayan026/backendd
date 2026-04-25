using FurnitureShop.Application.Dtos.Product;
namespace FurnitureShop.Application.Dtos.Cart;
public class CartItemDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImage { get; set; }
    public decimal? ProductPrice { get; set; }
    public int? CollectionId { get; set; }
    public string? CollectionName { get; set; }
    public string? CollectionImage { get; set; }
    public decimal? CollectionPrice { get; set; }
    public string? SelectedColor { get; set; }
    public string? SelectedSize { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}