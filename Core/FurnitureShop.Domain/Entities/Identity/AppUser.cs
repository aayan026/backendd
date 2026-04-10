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

    // RefreshToken plain text saxlanmır — SHA-256 hash-i DB-ə yazılır
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    // Hash metodu — AuthService istifadə edir
    public static string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
