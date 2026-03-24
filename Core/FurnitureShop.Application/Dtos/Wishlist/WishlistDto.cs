namespace FurnitureShop.Application.Dtos.Wishlist;
public class WishlistDto
{
    public int Id { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
    public int ItemCount => Items.Count;
}
