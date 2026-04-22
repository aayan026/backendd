using FurnitureShop.Application.Dtos.Auth;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IAuthService
{
    Task<TokenResponseDto> LoginAsync(LoginDto dto);
    Task<TokenResponseDto> RegisterAsync(RegisterDto dto);
    Task<TokenResponseDto> RefreshTokenAsync(TokenResponseDto refreshToken);
    Task LogoutAsync(string userId);
    Task ForgotPasswordAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);

    Task<TokenResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
}
