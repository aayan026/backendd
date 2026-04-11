using FurnitureShop.Domain.Entities.Identity;
using FurnitureShop.Persistence.Datas;
using Microsoft.AspNetCore.Identity;

namespace FurnitureShop.API.Extensions;

public static class SeedExtensions
{
    /// <summary>
    /// Rol və Admin seed-i — tətbiq başlayanda bir dəfə işləyir.
    /// </summary>
    public static async Task SeedRolesAndAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var config      = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // ── Rollar ──────────────────────────────────────────────────────
        foreach (var roleName in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new AppRole
                {
                    Id             = Guid.NewGuid().ToString(),
                    Name           = roleName,
                    NormalizedName = roleName.ToUpper()
                });
        }

        // ── Admin istifadəçisi ───────────────────────────────────────────
        var adminEmail    = config["SeedAdmin:Email"]    ?? "admin@furnitureshop.az";
        var adminPassword = config["SeedAdmin:Password"];

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null && !string.IsNullOrWhiteSpace(adminPassword))
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email    = adminEmail,
                Name     = "Admin",
                Surname  = "Admin"
            };
            await userManager.CreateAsync(adminUser, adminPassword);
        }

        if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
