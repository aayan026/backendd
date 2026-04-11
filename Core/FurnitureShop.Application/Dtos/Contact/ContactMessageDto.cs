namespace FurnitureShop.Application.Dtos.Contact;

public class ContactMessageDto
{
    public string Name    { get; set; } = null!;
    public string Email   { get; set; } = null!;
    public string? Phone  { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = null!;
    public string? Lang   { get; set; } = "az";
}
