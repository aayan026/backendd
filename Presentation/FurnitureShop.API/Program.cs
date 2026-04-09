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

// ── Bootstrap logger — proqram başlamazdan əvvəlki xətalar üçün ──────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("FurnitureShop API başlayır...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog konfiqurasiyası ───────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, config) =>
    {
        var seqUrl = context.Configuration["Seq:Url"] ?? "http://localhost:5341";

        config
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

            // Hər loga əlavə məlumat qoş
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "FurnitureShop.API")

            // 1. Console — development-də oxumaq üçün
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")

            // 2. Fayl — bütün loglar (gündəlik rotate, 30 gün saxla)
            .WriteTo.File(
                path: "logs/furnitureshop-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 50_000_000,
                rollOnFileSizeLimit: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{Application}] [Thread:{ThreadId}] {Message:lj}  {Properties:j}{NewLine}{Exception}")

            // 3. Yalnız Error+ loglar ayrıca faylda
            .WriteTo.File(
                path: "logs/errors-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}  {Properties:j}{NewLine}{Exception}")

            // 4. Seq — vizual UI (http://localhost:5341)
            .WriteTo.Seq(seqUrl);
    });

    var jwtSecret = builder.Configuration["JWT:Secret"];
    if (string.IsNullOrWhiteSpace(jwtSecret))
        throw new InvalidOperationException(
            "JWT:Secret konfiqurasiya edilməyib. appsettings.Development.json-a əlavə edin.");

    // ── Services ──────────────────────────────────────────────────────────────
    builder.Services.AddApplicationRegister();
    builder.Services.AddPersistenceRegister(builder.Configuration);
    builder.Services.AddInfrastructureRegister(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ILanguageService, LanguageService>();

    // ── Identity ──────────────────────────────────────────────────────────────
    builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // ── Validation ────────────────────────────────────────────────────────────
    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy
                .WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // ── Swagger ───────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "FurnitureShop API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT token daxil edin: Bearer {token}"
        });

        c.AddSecurityDefinition("Accept-Language", new OpenApiSecurityScheme
        {
            Name = "Accept-Language",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
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

    // ── Serilog HTTP Request logging — hər request avtomatik loglanır ─────────
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} => {StatusCode} ({Elapsed:0.000}ms)";

        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
            if (httpContext.User.Identity?.IsAuthenticated == true)
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "anonymous");
        };
    });

    // ── Seed: Roles + Admin ───────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        foreach (var roleName in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new AppRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
        }

        var adminEmail = builder.Configuration["SeedAdmin:Email"] ?? "admin@furnitureshop.az";
        var adminPassword = builder.Configuration["SeedAdmin:Password"];
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null && !string.IsNullOrWhiteSpace(adminPassword))
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Admin",
                Surname = "Admin"
            };
            await userManager.CreateAsync(adminUser, adminPassword);
        }

        if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // ── Middleware Pipeline ───────────────────────────────────────────────────
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseGlobalExceptionHandler();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("FurnitureShop API uğurla başladı");
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "API başlaya bilmədi — kritik xəta");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
