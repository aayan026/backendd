using FurnitureShop.Application.Dtos.Auth;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
        => OkResponse(await _authService.LoginAsync(dto));

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        => CreatedResponse(await _authService.RegisterAsync(dto));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenResponseDto token)
        => OkResponse(await _authService.RefreshTokenAsync(token));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync(UserId);
        return OkResponse(new { });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto);
        return OkResponse(new { });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return UpdatedResponse();
    }
}
