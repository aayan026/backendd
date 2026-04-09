using FurnitureShop.Application.Dtos.Auth;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Identity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService        _tokenService;
    private readonly IEmailService        _emailService;
    private readonly ILanguageService     _langService;
    private readonly IConfiguration      _config;
    private static readonly ILogger _log = Log.ForContext<AuthService>();

    private string Lang => _langService.GetCurrentLanguage();

    public AuthService(
        UserManager<AppUser> userManager,
        ITokenService        tokenService,
        IEmailService        emailService,
        ILanguageService     langService,
        IConfiguration       config)
    {
        _userManager  = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _langService  = langService;
        _config       = config;
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
    {
        _log.Information("Login cəhdi — Email: {Email}", dto.Email);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _log.Warning("Login uğursuz — İstifadəçi tapılmadı — Email: {Email}", dto.Email);
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "InvalidCredentials"));
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            _log.Warning("Login uğursuz — Hesab kilidlənib — UserId: {UserId} Email: {Email}", user.Id, dto.Email);
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "AccountLocked"));
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            _log.Warning("Login uğursuz — Yanlış şifrə — UserId: {UserId} Email: {Email}", user.Id, dto.Email);
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "InvalidCredentials"));
        }

        var result = await CreateTokenAndSaveAsync(user);
        _log.Information("Login uğurlu — UserId: {UserId} Email: {Email}", user.Id, user.Email);
        return result;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterDto dto)
    {
        _log.Information("Qeydiyyat cəhdi — Email: {Email} Ad: {Name} {Surname}", dto.Email, dto.Name, dto.Surname);

        var user = new AppUser
        {
            UserName    = dto.Email,
            Email       = dto.Email,
            Name        = dto.Name,
            Surname     = dto.Surname,
            PhoneNumber = dto.Phone
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code.ToLower())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => ValidationMessages.Get(Lang, e.Code)).ToList());

            _log.Warning("Qeydiyyat uğursuz — Email: {Email} — Xətalar: {Errors}", dto.Email, errors);
            throw new Application.Exceptions.ValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        var tokenResult = await CreateTokenAndSaveAsync(user);
        _log.Information("Qeydiyyat uğurlu — UserId: {UserId} Email: {Email}", user.Id, user.Email);
        return tokenResult;
    }

    public async Task<TokenResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
    {
        _log.Information("Google login cəhdi");

        var clientId = _config["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId) || clientId.Contains("YOUR_GOOGLE"))
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "GoogleNotConfigured"));

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            _log.Warning("Google login uğursuz — Token etibarsızdır");
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "GoogleTokenInvalid"));
        }

        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user is null)
        {
            _log.Information("Google login — Yeni istifadəçi yaradılır — Email: {Email}", payload.Email);
            var nameParts = (payload.Name ?? "").Split(' ', 2);
            user = new AppUser
            {
                UserName       = payload.Email,
                Email          = payload.Email,
                EmailConfirmed = true,
                Name           = nameParts.Length > 0 ? nameParts[0] : payload.Email,
                Surname        = nameParts.Length > 1 ? nameParts[1] : string.Empty
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(e => e.Code.ToLower())
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => ValidationMessages.Get(Lang, e.Code)).ToList());
                _log.Warning("Google login — İstifadəçi yaradılarkən xəta — Email: {Email} Xətalar: {Errors}", payload.Email, errors);
                throw new Application.Exceptions.ValidationException(errors);
            }

            await _userManager.AddToRoleAsync(user, "Customer");
            _log.Information("Google login — Yeni istifadəçi yaradıldı — UserId: {UserId} Email: {Email}", user.Id, user.Email);
        }
        else
        {
            if (await _userManager.IsLockedOutAsync(user))
            {
                _log.Warning("Google login uğursuz — Hesab kilidlənib — UserId: {UserId}", user.Id);
                throw new UnauthorizedException(ValidationMessages.Get(Lang, "AccountLocked"));
            }
        }

        var tokenResult = await CreateTokenAndSaveAsync(user);
        _log.Information("Google login uğurlu — UserId: {UserId} Email: {Email}", user.Id, user.Email);
        return tokenResult;
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(TokenResponseDto request)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken);

        if (user is null)
        {
            _log.Warning("Token yenilənməsi uğursuz — Token tapılmadı");
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "InvalidRefreshToken"));
        }

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            _log.Warning("Token yenilənməsi uğursuz — Token müddəti bitib — UserId: {UserId}", user.Id);
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "RefreshTokenExpired"));
        }

        var newAccessToken  = await _tokenService.CreateAccessTokenAsync(user);
        var newRefreshToken = _tokenService.CreateRefreshToken();

        user.RefreshToken           = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        _log.Information("Token yeniləndi — UserId: {UserId}", user.Id);
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

        _log.Information("İstifadəçi çıxış etdi — UserId: {UserId} Email: {Email}", userId, user.Email);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        _log.Information("Şifrə sıfırlama tələbi — Email: {Email}", dto.Email);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _log.Warning("Şifrə sıfırlama — İstifadəçi tapılmadı — Email: {Email}", dto.Email);
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        try
        {
            await _emailService.SendForgotPasswordAsync(
                user.Email!, $"{user.Name} {user.Surname}", token, Lang);
            _log.Information("Şifrə sıfırlama emaili göndərildi — UserId: {UserId} Email: {Email}", user.Id, user.Email);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Şifrə sıfırlama emaili göndərilə bilmədi — UserId: {UserId}", user.Id);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        _log.Information("Şifrə sıfırlanır — Email: {Email}", dto.Email);

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
                    g => g.Select(e => ValidationMessages.Get(Lang, e.Code)).ToList());
            _log.Warning("Şifrə sıfırlanarkən xəta — UserId: {UserId} Xətalar: {Errors}", user.Id, errors);
            throw new Application.Exceptions.ValidationException(errors);
        }

        _log.Information("Şifrə uğurla sıfırlandı — UserId: {UserId} Email: {Email}", user.Id, user.Email);
    }

    private async Task<TokenResponseDto> CreateTokenAndSaveAsync(AppUser user)
    {
        var tokenResponse = await _tokenService.CreateTokenAsync(user);
        user.RefreshToken           = tokenResponse.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);
        return tokenResponse;
    }
}
