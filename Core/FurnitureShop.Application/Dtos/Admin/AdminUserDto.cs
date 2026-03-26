namespace FurnitureShop.Application.Dtos.Admin;

public class AdminUserDto
{
    public string  Id          { get; set; } = null!;
    public string  Name        { get; set; } = null!;
    public string  Surname     { get; set; } = null!;
    public string  Email       { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public bool    IsLocked    { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
