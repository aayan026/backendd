using FurnitureShop.Application;
using FluentValidation;
using FurnitureShop.API.Middlewares;
using FurnitureShop.API.Services;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation.Concrete;
using FurnitureShop.API.Filters;
using FurnitureShop.Domain.Entities.Identity;
using FurnitureShop.Infrastructure;
using FurnitureShop.Persistence;
using FurnitureShop.Persistence.Datas;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// ── Serilog bootstrap logger ──────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("FurnitureShop API başlayır...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/furnitureshop-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

    // ── JWT Secret check ──────────────────────────────────────────────────────
    var jwtSecret = builder.Configuration["JWT:Secret"];
    if (string.IsNullOrWhiteSpace(jwtSecret))
        throw new InvalidOperationException(
            "JWT:Secret konfiqurasiya edilməyib. " +
            "appsettings.Development.json-a əlavə edin və ya JWT__Secret environment variable təyin edin.");

    // ── Persistence + Infrastructure ─────────────────────────────────────────
    builder.Services.AddApplicationRegister();
    builder.Services.AddPersistenceRegister(builder.Configuration);
    builder.Services.AddInfrastructureRegister(builder.Configuration);

    // ── Language Service ──────────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ILanguageService, LanguageService>();

    // ── Identity ──────────────────────────────────────────────────────────────
    builder.Services.AddIdentityCore<AppUser>(options =>
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

    // ── Controllers + FluentValidation ────────────────────────────────────────
    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // ── Static Files (file upload üçün) ──────────────────────────────────────
    builder.Services.AddDirectoryBrowser();

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // ── Swagger ───────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "FurnitureShop API",
            Version     = "v1",
            Description = "Furniture e-commerce backend API"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name        = "Authorization",
            Type        = SecuritySchemeType.Http,
            Scheme      = "bearer",
            BearerFormat= "JWT",
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
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // ── Seed: Roles + Admin user ──────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

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

        var adminEmail    = builder.Configuration["SeedAdmin:Email"] ?? "admin@furnitureshop.az";
        var adminPassword = builder.Configuration["SeedAdmin:Password"];
        var adminUser     = await userManager.FindByEmailAsync(adminEmail);

        if (!string.IsNullOrWhiteSpace(adminPassword))
        {
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Admin",
                    Surname = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                    Log.Warning("Admin seed xətası: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);

                if (!resetResult.Succeeded)
                    Log.Warning("Password reset xətası: {Errors}", string.Join(", ", resetResult.Errors.Select(e => e.Description)));
            }
        }
        if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // ── Middleware Pipeline ───────────────────────────────────────────────────
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseStaticFiles();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseGlobalExceptionHandler();

    app.UseCors("AllowFrontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application başlaya bilmədi");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
