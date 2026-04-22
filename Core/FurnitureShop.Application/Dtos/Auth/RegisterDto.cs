namespace FurnitureShop.Application.Dtos.Auth;
public class RegisterDto
{
    public string  Name            { get; set; } = null!;
    public string  Surname         { get; set; } = null!;
    public string  Email           { get; set; } = null!;
    public string  Password        { get; set; } = null!;
    public string  ConfirmPassword { get; set; } = null!;
    public string? Phone           { get; set; }
}
