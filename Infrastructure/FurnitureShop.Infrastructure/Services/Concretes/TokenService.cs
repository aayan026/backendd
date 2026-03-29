using FurnitureShop.Application.Dtos.Auth;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FurnitureShop.Infrastructure.Services.Concretes;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly string _jwtSecret;

    public TokenService(IConfiguration configuration, UserManager<AppUser> userManager)
    {
        _configuration = configuration;
        _userManager   = userManager;
        _jwtSecret     = _configuration["JWT:Secret"]
            ?? throw new InvalidOperationException("JWT:Secret is missing.");
    }

    public async Task<TokenResponseDto> CreateTokenAsync(AppUser user)
    {
        var accessToken = await CreateAccessTokenAsync(user);
        return new TokenResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = CreateRefreshToken(),
            ExpireDate   = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public string CreateRefreshToken()
    {
        var random = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);
        return Convert.ToBase64String(random);
    }

    public async Task<string> CreateAccessTokenAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            // null-safe: email boş ola bilər
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:            _configuration["JWT:Issuer"],
            audience:          _configuration["JWT:Audience"],
            claims:            claims,
            expires:           DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
