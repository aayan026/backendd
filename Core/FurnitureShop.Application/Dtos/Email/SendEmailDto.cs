namespace FurnitureShop.Application.Dtos.Email;

public class SendEmailDto
{
    public string ToEmail { get; set; } = null!;
    public string ToName { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public bool IsHtml { get; set; } = true;
}
