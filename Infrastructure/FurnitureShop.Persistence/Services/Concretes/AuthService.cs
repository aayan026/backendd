using FurnitureShop.Application.Dtos.Auth;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Services.Concretes;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService        _tokenService;
    private readonly IEmailService        _emailService;
    private readonly ILanguageService     _langService;

    private string Lang => _langService.GetCurrentLanguage();

    public AuthService(
        UserManager<AppUser> userManager,
        ITokenService        tokenService,
        IEmailService        emailService,
        ILanguageService     langService)
    {
        _userManager  = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _langService  = langService;
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "InvalidCredentials"));

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "InvalidCredentials"));

        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "AccountLocked"));

        var tokenResponse = await _tokenService.CreateTokenAsync(user);

        user.RefreshToken           = tokenResponse.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return tokenResponse;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterDto dto)
    {
        var user = new AppUser
        {
            UserName = dto.Email,
            Email    = dto.Email,
            Name     = dto.Name,
            Surname  = dto.Surname
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code.ToLower())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => ValidationMessages.Get(Lang, e.Code)).ToList()
                );
            throw new Application.Exceptions.ValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        var tokenResponse = await _tokenService.CreateTokenAsync(user);

        user.RefreshToken           = tokenResponse.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return tokenResponse;
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(TokenResponseDto request)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken);

        if (user is null)
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "InvalidRefreshToken"));

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "RefreshTokenExpired"));

        var newAccessToken  = await _tokenService.CreateAccessTokenAsync(user);
        var newRefreshToken = _tokenService.CreateRefreshToken();

        user.RefreshToken           = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new TokenResponseDto
        {
            AccessToken  = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpireDate   = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        user.RefreshToken           = null;
        user.RefreshTokenExpiryTime = DateTime.MinValue;
        await _userManager.UpdateAsync(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        _ = _emailService.SendForgotPasswordAsync(
            user.Email!, $"{user.Name} {user.Surname}", token, Lang);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code.ToLower())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => ValidationMessages.Get(Lang, e.Code)).ToList()
                );
            throw new Application.Exceptions.ValidationException(errors);
        }
    }
}
