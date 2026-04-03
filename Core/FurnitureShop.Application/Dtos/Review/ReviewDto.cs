namespace FurnitureShop.Application.Dtos.Review;

public class ReviewDto
{
    public int    Id          { get; set; }
    public int    ProductId   { get; set; }
    public string AuthorName  { get; set; } = null!;
    public int    Rating      { get; set; }
    public string Comment     { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
