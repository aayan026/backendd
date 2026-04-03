using FurnitureShop.Domain.Entities.Common;

namespace FurnitureShop.Domain.Entities.Concretes;

public class Review : BaseEntity
{
    public int     ProductId   { get; set; }
    public Product Product     { get; set; } = null!;

    public string  AuthorName  { get; set; } = null!;
    public string? AuthorEmail { get; set; }
    public int     Rating      { get; set; }   
    public string  Comment     { get; set; } = null!;
    public bool    IsApproved  { get; set; } = true;
}
