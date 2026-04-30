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

SerilogExtensions.ConfigureBootstrapLogger();

try
{
    Log.Information("FurnitureShop API başlayır...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddSerilogConfiguration();

    if (string.IsNullOrWhiteSpace(builder.Configuration["JWT:Secret"]))
        throw new InvalidOperationException(
            "JWT:Secret konfiqurasiya edilməyib. .env faylına JWT__Secret əlavə edin.");

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
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    var app = builder.Build();

    await app.SeedRolesAndAdminAsync();

    app.UseSerilogHttpLogging();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseGlobalExceptionHandler();

    app.Use(async (context, next) =>
    {
        context.Request.EnableBuffering();
        await next();
    });
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
