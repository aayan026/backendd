namespace FurnitureShop.Application.Dtos.Wishlist;
public class WishlistItemDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImage { get; set; }
    public decimal? ProductPrice { get; set; }
    public int? CollectionId { get; set; }
    public string? CollectionName { get; set; }
}
