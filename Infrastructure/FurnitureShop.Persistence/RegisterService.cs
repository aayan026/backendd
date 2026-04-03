using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Identity;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.ReadRepositories;
using FurnitureShop.Persistence.Repositories.WriteRepositories;
using FurnitureShop.Persistence.Repositories.ReadRepositories;
using FurnitureShop.Persistence.Repositories.WriteRepositories;
using FurnitureShop.Persistence.Services.Concretes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FurnitureShop.Persistence;

public static class RegisterService
{
    public static void AddPersistenceRegister(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        // ── Memory Cache ───────────────────────────────────────────────────
        services.AddMemoryCache();

        AddRepositoriesExtension(services);
        AddServicesExtension(services);
    }

    private static void AddRepositoriesExtension(IServiceCollection services)
    {
        // Read Repositories
        services.AddScoped<IProductReadRepository,             ProductReadRepository>();
        services.AddScoped<IFurnitureCategoryReadRepository,   FurnitureCategoryReadRepository>();
        services.AddScoped<ICollectionReadRepository,          CollectionReadRepository>();
        services.AddScoped<ICollectionCategoryReadRepository,  CollectionCategoryReadRepository>();
        services.AddScoped<IOrderReadRepository,               OrderReadRepository>();
        services.AddScoped<IAddressReadRepository,             AddressReadRepository>();
        services.AddScoped<ICartReadRepository,                CartReadRepository>();
        services.AddScoped<IWishlistReadRepository,            WishlistReadRepository>();
        services.AddScoped<ICampaignReadRepository,            CampaignReadRepository>();
        services.AddScoped<IHeroSectionReadRepository,         HeroSectionReadRepository>();
        services.AddScoped<IDiscountCodeReadRepository,        DiscountCodeReadRepository>();
        services.AddScoped<IReviewReadRepository,              ReviewReadRepository>();

        // Write Repositories
        services.AddScoped<IProductWriteRepository,            ProductWriteRepository>();
        services.AddScoped<IFurnitureCategoryWriteRepository,  FurnitureCategoryWriteRepository>();
        services.AddScoped<ICollectionWriteRepository,         CollectionWriteRepository>();
        services.AddScoped<ICollectionCategoryWriteRepository, CollectionCategoryWriteRepository>();
        services.AddScoped<IOrderWriteRepository,              OrderWriteRepository>();
        services.AddScoped<IAddressWriteRepository,            AddressWriteRepository>();
        services.AddScoped<ICartWriteRepository,               CartWriteRepository>();
        services.AddScoped<IWishlistWriteRepository,           WishlistWriteRepository>();
        services.AddScoped<ICampaignWriteRepository,           CampaignWriteRepository>();
        services.AddScoped<IHeroSectionWriteRepository,        HeroSectionWriteRepository>();
        services.AddScoped<IDiscountCodeWriteRepository,       DiscountCodeWriteRepository>();
        services.AddScoped<IReviewWriteRepository,             ReviewWriteRepository>();
    }

    private static void AddServicesExtension(IServiceCollection services)
    {
        services.AddScoped<IProductService,           ProductService>();
        services.AddScoped<IFurnitureCategoryService, FurnitureCategoryService>();
        services.AddScoped<ICollectionService,        CollectionService>();
        services.AddScoped<ICollectionCategoryService,CollectionCategoryService>();
        services.AddScoped<IOrderService,             OrderService>();
        services.AddScoped<IAuthService,              AuthService>();
        services.AddScoped<ICampaignService,          CampaignService>();
        services.AddScoped<IHeroSectionService,       HeroSectionService>();
        services.AddScoped<IDiscountCodeService,      DiscountCodeService>();
        services.AddScoped<ICartService,              CartService>();
        services.AddScoped<IWishlistService,          WishlistService>();
        services.AddScoped<IAddressService,           AddressService>();
        services.AddScoped<IAdminService,            AdminService>();
        services.AddScoped<IReviewService,           ReviewService>();
    }
}
