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
        CreateMap<ProductImage, ProductImageDto>();

        CreateMap<ProductColor, ProductColorDto>();

        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Name,         o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Name))
            .ForMember(d => d.Description,  o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Description))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s =>
                s.FurnitureCategory != null
                    ? s.FurnitureCategory.Translations.FirstOrDefault()!.Name
                    : null))
            .ForMember(d => d.Images,  o => o.MapFrom(s => s.Images))
            .ForMember(d => d.Colors,  o => o.MapFrom(s => s.Colors));

        // CreateProductDto → Product
        CreateMap<CreateProductDto, Product>()
            .ForMember(d => d.Images,      o => o.Ignore())  // servis əl ilə qurur
            .ForMember(d => d.Colors,      o => o.Ignore()) 
            .ForMember(d => d.Translations,o => o.Ignore()); // servis əl ilə qurur

        // UpdateProductDto → Product
        CreateMap<UpdateProductDto, Product>()
            .ForMember(d => d.Images,o => o.Ignore())
            .ForMember(d => d.Colors,o => o.Ignore())
            .ForMember(d => d.Translations,o => o.Ignore());

        // CreateProductColorDto → ProductColor
        CreateMap<CreateProductColorDto, ProductColor>();

        // ProductTranslationDto → ProductTranslation
        CreateMap<ProductTranslationDto, ProductTranslation>();

        // ── FurnitureCategory ─────────────────────────────────────────────
        CreateMap<FurnitureCategory, FurnitureCategoryDto>()
            .ForMember(d => d.Name,     o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Name))
            .ForMember(d => d.ImageUrl, o => o.Ignore()); // entity-də ImageUrl yoxdur

        CreateMap<CreateFurnitureCategoryDto, FurnitureCategory>()
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<UpdateFurnitureCategoryDto, FurnitureCategory>()
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<FurnitureCategoryTranslationDto, FurnitureCategoryTranslation>()
            .ForMember(d => d.Lang, o => o.MapFrom(s => s.Lang));

        // ── Collection ────────────────────────────────────────────────────
        CreateMap<Collection, CollectionDto>()
            .ForMember(d => d.Name,         o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Name))
            .ForMember(d => d.Description,  o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Description))
            .ForMember(d => d.ImageUrl,     o => o.MapFrom(s => s.ImagesUrl))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s =>
                s.CollectionCategory != null
                    ? s.CollectionCategory.Translations.FirstOrDefault()!.Name
                    : null))
            .ForMember(d => d.Products, o => o.MapFrom(s => s.Products));

        CreateMap<CreateCollectionDto, Collection>()
            .ForMember(d => d.ImagesUrl,    o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.Translations, o => o.Ignore())
            .ForMember(d => d.Products,     o => o.Ignore());

        CreateMap<UpdateCollectionDto, Collection>()
            .ForMember(d => d.ImagesUrl,    o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.Translations, o => o.Ignore())
            .ForMember(d => d.Products,     o => o.Ignore());

        CreateMap<CollectionTranslationDto, CollectionTranslation>();

        // ── CollectionCategory ────────────────────────────────────────────
        CreateMap<CollectionCategory, CollectionCategoryDto>()
            .ForMember(d => d.Name,     o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Name))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl));

        CreateMap<CreateCollectionCategoryDto, CollectionCategory>()
            .ForMember(d => d.ImageUrl,    o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<UpdateCollectionCategoryDto, CollectionCategory>()
            .ForMember(d => d.ImageUrl,    o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<CollectionCategoryTranslationDto, CollectionCategoryTranslation>();

        // ── Order ─────────────────────────────────────────────────────────
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName,  o => o.MapFrom(s =>
                s.Product != null ? s.Product.Translations.FirstOrDefault()!.Name : null))
            .ForMember(d => d.ProductImage, o => o.MapFrom(s =>
                s.Product != null ? s.Product.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl : null))
            .ForMember(d => d.CollectionName, o => o.MapFrom(s =>
                s.Collection != null ? s.Collection.Translations.FirstOrDefault()!.Name : null));

        CreateMap<DeliveryInfo, DeliveryInfoDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.UserFullName, o => o.MapFrom(s =>
                s.User != null ? $"{s.User.Name} {s.User.Surname}" : ""))
            .ForMember(d => d.DiscountCode, o => o.MapFrom(s =>
                s.DiscountCode != null ? s.DiscountCode.Code : null))
            .ForMember(d => d.Items,        o => o.MapFrom(s => s.Items))
            .ForMember(d => d.DeliveryInfo, o => o.MapFrom(s => s.DeliveryInfo));

        CreateMap<CreateDeliveryInfoDto, DeliveryInfo>();

        CreateMap<CreateOrderItemDto, OrderItem>();

        CreateMap<CreateOrderDto, Order>()
            .ForMember(d => d.Items,        o => o.MapFrom(s => s.Items))
            .ForMember(d => d.DeliveryInfo, o => o.Ignore()); // ayrıca map edilir

        // ── Address ───────────────────────────────────────────────────────
        CreateMap<Address, AddressDto>();
        CreateMap<CreateAddressDto, Address>();
        CreateMap<UpdateAddressDto, Address>();

        // ── Cart ──────────────────────────────────────────────────────────
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName,    o => o.MapFrom(s =>
                s.Product != null ? s.Product.Translations.FirstOrDefault()!.Name : null))
            .ForMember(d => d.ProductImage,   o => o.MapFrom(s =>
                s.Product != null ? s.Product.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl : null))
            .ForMember(d => d.ProductPrice,   o => o.MapFrom(s =>
                s.Product != null ? s.Product.Price : (decimal?)null))
            .ForMember(d => d.CollectionName, o => o.MapFrom(s =>
                s.Collection != null ? s.Collection.Translations.FirstOrDefault()!.Name : null))
            .ForMember(d => d.CollectionPrice,o => o.MapFrom(s =>
                s.Collection != null ? s.Collection.TotalPrice : (decimal?)null))
            .ForMember(d => d.TotalPrice,     o => o.MapFrom(s =>
                s.Product != null
                    ? s.Product.Price * s.Quantity
                    : s.Collection != null
                        ? s.Collection.TotalPrice * s.Quantity
                        : 0));

        CreateMap<Cart, CartDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

       CreateMap<WishlistItem, WishlistItemDto>()
            .ForMember(d => d.ProductName,    o => o.MapFrom(s =>
                s.Product != null ? s.Product.Translations.FirstOrDefault()!.Name : null))
            .ForMember(d => d.ProductImage,   o => o.MapFrom(s =>
                s.Product != null ? s.Product.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl : null))
            .ForMember(d => d.ProductPrice,   o => o.MapFrom(s =>
                s.Product != null ? s.Product.Price : (decimal?)null))
            .ForMember(d => d.CollectionName, o => o.MapFrom(s =>
                s.Collection != null ? s.Collection.Translations.FirstOrDefault()!.Name : null));

        CreateMap<Wishlist, WishlistDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        CreateMap<Campaign, CampaignDto>()
            .ForMember(d => d.Title,       o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Title))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Description))
            .ForMember(d => d.ButtonText,  o => o.MapFrom(s => s.Translations.FirstOrDefault()!.ButtonText));

        CreateMap<CreateCampaignDto, Campaign>()
            .ForMember(d => d.Translations, o => o.Ignore());

        // ── HeroSection ───────────────────────────────────────────────────
        CreateMap<HeroSection, HeroSectionDto>()
            .ForMember(d => d.Title,    o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Title))
            .ForMember(d => d.Subtitle, o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Subtitle))
            .ForMember(d => d.BadgeText,o => o.MapFrom(s => s.Translations.FirstOrDefault()!.BadgeText));

        CreateMap<CreateHeroSectionDto, HeroSection>()
            .ForMember(d => d.Translations, o => o.Ignore());

        CreateMap<CreateDiscountCodeDto, DiscountCode>();

        CreateMap<DiscountCode, DiscountCodeDto>();

    }
}
