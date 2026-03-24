using FurnitureShop.Application;
using FluentValidation;
using FluentValidation.AspNetCore;
using FurnitureShop.API.Middlewares;
using FurnitureShop.API.Services;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Identity;
using FurnitureShop.Infrastructure;
using FurnitureShop.Persistence;
using FurnitureShop.Persistence.Datas;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using FurnitureShop.Application.Validation.Concrete;
using FurnitureShop.API.Filters;

var builder = WebApplication.CreateBuilder(args);
var jwtSecret = builder.Configuration["JWT:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("JWT:Secret is not configured. Set JWT__Secret environment variable.");

// ── Persistence + Infrastructure ──────────────────────────────────────────
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
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<AppRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── Controllers + FluentValidation ───────────────────────────────────────
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
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000"
            )
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

// ── Seed: Admin rol və istifadəçi (yalniz development) ───────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new AppRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Admin",
            NormalizedName = "ADMIN"
        });

    if (!await roleManager.RoleExistsAsync("Customer"))
        await roleManager.CreateAsync(new AppRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Customer",
            NormalizedName = "CUSTOMER"
        });

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

// ── Middleware pipeline ────────────────────────────────────────────────────

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseGlobalExceptionHandler();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();