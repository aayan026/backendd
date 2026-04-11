using Microsoft.OpenApi.Models;

namespace FurnitureShop.API.Extensions;

public static class SwaggerExtensions
{
    public static void AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "FurnitureShop API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = SecuritySchemeType.Http,
                Scheme      = "bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "JWT token daxil edin: Bearer {token}"
            });

            c.AddSecurityDefinition("Accept-Language", new OpenApiSecurityScheme
            {
                Name        = "Accept-Language",
                Type        = SecuritySchemeType.ApiKey,
                In          = ParameterLocation.Header,
                Description = "Dil seçimi: az (default), ru, en"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}
