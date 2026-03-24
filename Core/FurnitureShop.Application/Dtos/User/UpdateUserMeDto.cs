namespace FurnitureShop.Application.Dtos.User;

public class UpdateUserMeDto
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}
