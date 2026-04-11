using FluentValidation;
using FurnitureShop.API.Extensions;
using FurnitureShop.API.Filters;
using FurnitureShop.API.Middlewares;
using FurnitureShop.API.Services;
using FurnitureShop.Application;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation.Concrete;
using FurnitureShop.Infrastructure;
using FurnitureShop.Persistence;
using Serilog;

// ── Bootstrap logger ─────────────────────────────────────────────────────────
SerilogExtensions.ConfigureBootstrapLogger();

try
{
    Log.Information("FurnitureShop API başlayır...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.AddSerilogConfiguration();

    // Validate JWT
    if (string.IsNullOrWhiteSpace(builder.Configuration["JWT:Secret"]))
        throw new InvalidOperationException(
            "JWT:Secret konfiqurasiya edilməyib. appsettings.Development.json-a əlavə edin.");

    // ── Service registrations ────────────────────────────────────────────────
    builder.Services.AddApplicationRegister();
    builder.Services.AddPersistenceRegister(builder.Configuration);
    builder.Services.AddInfrastructureRegister(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ILanguageService, LanguageService>();

    builder.Services.AddIdentityConfiguration();
    builder.Services.AddCorsConfiguration();
    builder.Services.AddSwaggerConfiguration();

    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // ── Build ────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Seed ─────────────────────────────────────────────────────────────────
    await app.SeedRolesAndAdminAsync();

    // ── Middleware Pipeline ───────────────────────────────────────────────────
    app.UseSerilogHttpLogging();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseGlobalExceptionHandler();
    app.UseAuthRateLimiting();
    app.UseCors(CorsExtensions.PolicyName);
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
