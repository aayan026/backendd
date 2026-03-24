namespace FurnitureShop.Application.Dtos.Auth;
public class TokenResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpireDate { get; set; }
}
