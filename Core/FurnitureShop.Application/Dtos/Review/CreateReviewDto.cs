namespace FurnitureShop.Application.Dtos.Review;

public class CreateReviewDto
{
    public int    ProductId   { get; set; }
    public string AuthorName  { get; set; } = null!;
    public string? AuthorEmail { get; set; }
    public int    Rating      { get; set; }   // 1–5
    public string Comment     { get; set; } = null!;
}
