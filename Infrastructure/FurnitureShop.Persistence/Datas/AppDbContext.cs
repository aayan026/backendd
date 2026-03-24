using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;
using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Datas;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Identity
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Catalog
    public DbSet<FurnitureCategory> FurnitureCategories => Set<FurnitureCategory>();
    public DbSet<FurnitureCategoryTranslation> FurnitureCategoryTranslations => Set<FurnitureCategoryTranslation>();
    public DbSet<CollectionCategory> CollectionCategories => Set<CollectionCategory>();
    public DbSet<CollectionCategoryTranslation> CollectionCategoryTranslations => Set<CollectionCategoryTranslation>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionTranslation> CollectionTranslations => Set<CollectionTranslation>();

    // Product
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductColor> ProductColors => Set<ProductColor>();

    // Order
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<DeliveryInfo> DeliveryInfos => Set<DeliveryInfo>();

    // User
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Marketing
    public DbSet<HeroSection> HeroSections => Set<HeroSection>();
    public DbSet<HeroTranslation> HeroTranslations => Set<HeroTranslation>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignTranslation> CampaignTranslations => Set<CampaignTranslation>();
    public DbSet<DiscountCode> DiscountCodes => Set<DiscountCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
