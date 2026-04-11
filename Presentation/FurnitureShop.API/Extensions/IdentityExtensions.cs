using FurnitureShop.Domain.Entities.Identity;
using FurnitureShop.Persistence.Datas;
using Microsoft.AspNetCore.Identity;

namespace FurnitureShop.API.Extensions;

public static class IdentityExtensions
{
    public static void AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit           = true;
            options.Password.RequireUppercase       = true;
            options.Password.RequiredLength         = 8;
            options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers      = true;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
    }
}
