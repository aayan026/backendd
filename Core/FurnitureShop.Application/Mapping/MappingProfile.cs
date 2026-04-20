using AutoMapper;
using FurnitureShop.Application.Dtos.Address;
using FurnitureShop.Application.Dtos.Campaign;
using FurnitureShop.Application.Dtos.Cart;
using FurnitureShop.Application.Dtos.Collection;
using FurnitureShop.Application.Dtos.CollectionCategory;
using FurnitureShop.Application.Dtos.DiscountCode;
using FurnitureShop.Application.Dtos.FurnitureCategory;
using FurnitureShop.Application.Dtos.HeroSection;
using FurnitureShop.Application.Dtos.Order;
using FurnitureShop.Application.Dtos.Product;
using FurnitureShop.Application.Dtos.Wishlist;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Concretes.Translation;

namespace FurnitureShop.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product 
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductColor, ProductColorDto>();

        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Name
                    : string.Empty))
            .ForMember(d => d.Description, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Description
                    : null))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s =>
                s.FurnitureCategory != null && s.FurnitureCategory.Translations.Any()
                    ? s.FurnitureCategory.Translations.First().Name
                    : null))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images))
            .ForMember(d => d.Colors, o => o.MapFrom(s => s.Colors));

        CreateMap<CreateProductDto, Product>()
            .ForMember(d => d.Images, o => o.Ignore())
            .ForMember(d => d.Colors, o => o.Ignore())
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<UpdateProductDto, Product>()
            .ForMember(d => d.Images, o => o.Ignore())
            .ForMember(d => d.Colors, o => o.Ignore())
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<CreateProductColorDto, ProductColor>();
        CreateMap<ProductTranslationDto, ProductTranslation>();

        // FurnitureCategory 
        CreateMap<FurnitureCategory, FurnitureCategoryDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Name
                    : string.Empty))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl));

        CreateMap<CreateFurnitureCategoryDto, FurnitureCategory>()
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<UpdateFurnitureCategoryDto, FurnitureCategory>()
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<FurnitureCategoryTranslationDto, FurnitureCategoryTranslation>();

        // Collection
        CreateMap<Collection, CollectionDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Name
                    : string.Empty))
            .ForMember(d => d.Description, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Description
                    : null))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImagesUrl))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s =>
                s.CollectionCategory != null && s.CollectionCategory.Translations.Any()
                    ? s.CollectionCategory.Translations.First().Name
                    : null))
            .ForMember(d => d.Products, o => o.MapFrom(s => s.Products));

        CreateMap<CreateCollectionDto, Collection>()
            .ForMember(d => d.ImagesUrl, o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.Translations, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore());

        CreateMap<UpdateCollectionDto, Collection>()
            .ForMember(d => d.ImagesUrl, o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.Translations, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore());

        CreateMap<CollectionTranslationDto, CollectionTranslation>();

        // CollectionCategory 
        CreateMap<CollectionCategory, CollectionCategoryDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Name
                    : string.Empty))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl));

        CreateMap<CreateCollectionCategoryDto, CollectionCategory>()
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<UpdateCollectionCategoryDto, CollectionCategory>()
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<CollectionCategoryTranslationDto, CollectionCategoryTranslation>();

        // ── Order ─────────────────────────────────────────────────────────
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s =>
                s.Product != null && s.Product.Translations.Any()
                    ? s.Product.Translations.First().Name
                    : null))
            .ForMember(d => d.ProductImage, o => o.MapFrom(s =>
                s.Product != null && s.Product.Images.Any(i => i.IsPrimary)
                    ? s.Product.Images.First(i => i.IsPrimary).ImageUrl
                    : null))
            .ForMember(d => d.CollectionName, o => o.MapFrom(s =>
                s.Collection != null && s.Collection.Translations.Any()
                    ? s.Collection.Translations.First().Name
                    : null));

        CreateMap<DeliveryInfo, DeliveryInfoDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.UserFullName, o => o.MapFrom(s =>
                s.User != null ? $"{s.User.Name} {s.User.Surname}" : ""))
            .ForMember(d => d.UserPhone, o => o.MapFrom(s =>
                s.User != null ? s.User.PhoneNumber : null))
            .ForMember(d => d.UserEmail, o => o.MapFrom(s =>
                s.User != null ? s.User.Email : null))
            .ForMember(d => d.DiscountCode, o => o.MapFrom(s =>
                s.DiscountCode != null ? s.DiscountCode.Code : null))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.DeliveryInfo, o => o.MapFrom(s => s.DeliveryInfo));

        CreateMap<CreateDeliveryInfoDto, DeliveryInfo>()
            .ForMember(d => d.ScheduledDate, o => o.MapFrom(s => s.ScheduledDate))
            .ForMember(d => d.TimeSlot, o => o.MapFrom(s => s.TimeSlot));
        CreateMap<CreateOrderItemDto, OrderItem>();
        CreateMap<CreateOrderDto, Order>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.TotalPrice))
            .ForMember(d => d.IsCustomOrder, o => o.MapFrom(s => s.IsCustomOrder))
            .ForMember(d => d.CustomDescription, o => o.MapFrom(s => s.CustomDescription))
            .ForMember(d => d.PaidAmount, o => o.MapFrom(s => s.PaidAmount))
            .ForMember(d => d.InstallmentMonths, o => o.MapFrom(s => s.InstallmentMonths))
            .ForMember(d => d.MonthlyPayment, o => o.MapFrom(s => s.MonthlyPayment))
            .ForMember(d => d.DeliveryInfo, o => o.Ignore());

        // ── Address ───────────────────────────────────────────────────────
        CreateMap<Address, AddressDto>();
        CreateMap<CreateAddressDto, Address>();
        CreateMap<UpdateAddressDto, Address>();

        // ── Cart ──────────────────────────────────────────────────────────
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s =>
                s.Product != null && s.Product.Translations.Any()
                    ? s.Product.Translations.First().Name
                    : null))
            .ForMember(d => d.ProductImage, o => o.MapFrom(s =>
                s.Product != null && s.Product.Images.Any(i => i.IsPrimary)
                    ? s.Product.Images.First(i => i.IsPrimary).ImageUrl
                    : null))
            .ForMember(d => d.ProductPrice, o => o.MapFrom(s =>
                s.Product != null ? s.Product.Price : (decimal?)null))
            .ForMember(d => d.CollectionName, o => o.MapFrom(s =>
                s.Collection != null && s.Collection.Translations.Any()
                    ? s.Collection.Translations.First().Name
                    : null))
            .ForMember(d => d.CollectionPrice, o => o.MapFrom(s =>
                s.Collection != null ? s.Collection.TotalPrice : (decimal?)null))
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s =>
                s.Product != null
                    ? s.Product.Price * s.Quantity
                    : s.Collection != null
                        ? s.Collection.TotalPrice * s.Quantity
                        : 0));

        CreateMap<Cart, CartDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        // ── Wishlist ──────────────────────────────────────────────────────
        CreateMap<WishlistItem, WishlistItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s =>
                s.Product != null && s.Product.Translations.Any()
                    ? s.Product.Translations.First().Name
                    : null))
            .ForMember(d => d.ProductImage, o => o.MapFrom(s =>
                s.Product != null && s.Product.Images.Any(i => i.IsPrimary)
                    ? s.Product.Images.First(i => i.IsPrimary).ImageUrl
                    : null))
            .ForMember(d => d.ProductPrice, o => o.MapFrom(s =>
                s.Product != null ? s.Product.Price : (decimal?)null))
            .ForMember(d => d.CollectionName, o => o.MapFrom(s =>
                s.Collection != null && s.Collection.Translations.Any()
                    ? s.Collection.Translations.First().Name
                    : null));

        CreateMap<Wishlist, WishlistDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        // ── Campaign ──────────────────────────────────────────────────────
        CreateMap<Campaign, CampaignDto>()
            .ForMember(d => d.Title, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Title
                    : string.Empty))
            .ForMember(d => d.Description, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Description
                    : null))
            .ForMember(d => d.ButtonText, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().ButtonText
                    : null))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive));

        CreateMap<CreateCampaignDto, Campaign>()
            .ForMember(d => d.Translations, o => o.Ignore());

        // ── HeroSection ───────────────────────────────────────────────────
        CreateMap<HeroSection, HeroSectionDto>()
            .ForMember(d => d.Title, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Title
                    : string.Empty))
            .ForMember(d => d.CollectionId, o => o.MapFrom(s => s.CollectionId))
            .ForMember(d => d.Subtitle, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().Subtitle
                    : null))
            .ForMember(d => d.BadgeText, o => o.MapFrom(s =>
                s.Translations.FirstOrDefault() != null
                    ? s.Translations.First().BadgeText
                    : null))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive));

        CreateMap<CreateHeroSectionDto, HeroSection>()
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<CreateDiscountCodeDto, DiscountCode>();
        CreateMap<DiscountCode, DiscountCodeDto>();
    }
}