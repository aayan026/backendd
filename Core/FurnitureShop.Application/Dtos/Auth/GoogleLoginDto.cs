namespace FurnitureShop.Application.Dtos.Auth;

public class GoogleLoginDto
{
    /// <summary>
    /// Frontend-dən gələn Google ID Token (google.accounts.id.initialize callback-indən)
    /// </summary>
    public string IdToken { get; set; } = null!;
}
