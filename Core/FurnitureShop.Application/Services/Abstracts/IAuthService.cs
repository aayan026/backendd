using FurnitureShop.Application.Dtos.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Application.Services.Abstracts;
public interface IAuthService
{
    Task<TokenResponseDto> LoginAsync(LoginDto dto);
    Task<TokenResponseDto> RegisterAsync(RegisterDto dto);
    Task<TokenResponseDto> RefreshTokenAsync(TokenResponseDto refreshToken);
    Task LogoutAsync(string userId);
    Task ForgotPasswordAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
