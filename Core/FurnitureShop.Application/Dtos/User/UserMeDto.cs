namespace FurnitureShop.Application.Dtos.User;

public class UserMeDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
}
