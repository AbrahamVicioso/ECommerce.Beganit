using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Beganit.AdminPanel
{
    public static class MapsterConfig
    {
        public static void Configure()
        {
            TypeAdapterConfig<Brand,BrandViewModel>.NewConfig();
            TypeAdapterConfig<Category,CategoryViewModel>.NewConfig();
            TypeAdapterConfig<ProductVariant,ProductVariantViewModel>.NewConfig()
                .Map(dest => dest.ProductVariantAttributes, src => src.ProductVariantAttributes.Select(x => new ProductVariantAttributeViewModel()
                {
                    AttributeName = x.AttributeName,
                    AttributeValue = x.AttributeValue,
                    VariantId  = x.VariantId,
                }));
            TypeAdapterConfig<IdentityRole,IdentityRoleViewModel>.NewConfig();
            TypeAdapterConfig<Product, ProductViewModel>.NewConfig()
                .Map(dest => dest.Categories, src => src.Categories.Select(x => x.Name))
                .Map(dest => dest.Rating, src => src.Reviews.Any() ?
                    src.Reviews.Select(x => x.Rating | 0).Average() : 0)
                .Map(dest => dest.Attributes, src => src.ProductAttributes.Select(x => new ProductAttributeViewmodel()
                {
                    AttributeName = x.AttributeName,
                    AttributeValue = x.AttributeValue,
                    Id = x.Id,
                    ProductId = x.ProductId 
                })).Map(dest => dest.Variants, src => src.ProductVariants.Select(x => new ProductVariantViewModel()
                {
                    Id = x.Id,
                    IsActive = x.IsActive?? false,
                    Price = x.Price,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Sku = x.Sku,
                    ProductVariantAttributes = x.ProductVariantAttributes.Select(x => new ProductVariantAttributeViewModel()
                    {
                        AttributeName= x.AttributeName,
                        AttributeValue= x.AttributeValue,
                        VariantId = x.VariantId,
                    }).ToList()
                }))
                //.Map(dest => dest.Rating, src => src.Reviews.Select(x => x.Rating | 0).Average())
                .Map(dest => dest.Images, src => src.ProductImages.Select(x => new ProductImageViewModel()
                {
                    ImageUrl = x.ImageUrl,
                    AltText = x.AltText,    
                    CreatedAt = x.CreatedAt,
                    DisplayOrder = x.DisplayOrder
                }));
            TypeAdapterConfig<ProductViewModel, Product>
                .NewConfig()
                .Ignore(dest => dest.Categories);

            TypeAdapterConfig<ProductBundle, ProductBundleViewModel>.NewConfig()
                .Map(dest => dest.Items, src => src.ProductBundleItems.Select(x => new ProductBundleItemViewModel() {
                    BundleId = x.BundleId,
                    ProductId  = x.ProductId,
                    Quantity = x.Quantity,
                }));

            TypeAdapterConfig<ProductBundleItem, ProductBundleItemViewModel>.NewConfig();

            TypeAdapterConfig<Cart, CartViewModel>.NewConfig()
                .Map(dest => dest.CartItems, src => src.CartItems.Select(x => new CartItemViewModel()
                {
                    CartId = x.CartId,
                    CreatedAt = x.CreatedAt,
                    Id = x.Id,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UpdatedAt = x.UpdatedAt,
                    VariantId = x.VariantId
                }));

            TypeAdapterConfig<CartItem, CartItemViewModel>.NewConfig();

            TypeAdapterConfig<CustomerAddress, CustomerAddressViewModel>.NewConfig();

            TypeAdapterConfig<Order, OrderViewModel>.NewConfig();
            TypeAdapterConfig<OrderStatus, OrderStatusViewModel>.NewConfig();
            TypeAdapterConfig<OrderItem, OrderItemViewModel>.NewConfig();
            TypeAdapterConfig<OrderStatusHistory, OrderStatusHistoryViewModel>.NewConfig();

            TypeAdapterConfig<Review, ReviewViewModel>.NewConfig();

        }
    }
}
