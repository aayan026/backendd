using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Infrastructure.Services.Concretes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FurnitureShop.Infrastructure;

public static class RegisterService
{
    public static void AddInfrastructureRegister(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = configuration["JWT:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret))
            throw new InvalidOperationException("JWT:Secret must be configured via appsettings or environment variables.");

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileUploadService, CloudinaryFileUploadService>();


        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JWT:Issuer"],
                ValidAudience = configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret))
            };
        });
    }
}
