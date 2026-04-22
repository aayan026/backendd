using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace FurnitureShop.Domain.Entities.Identity;

public class AppUser : IdentityUser
{
    public AppUser()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    public static string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
