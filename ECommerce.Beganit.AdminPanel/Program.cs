using CloudinaryDotNet;
using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using ECommerce.Beganit.AdminPanel.Services;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ECommerce.Beganit.AdminPanel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ECommerceDBContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ECommerceDBContext>();

            builder.Services.AddControllersWithViews();

            Cloudinary cloudinary = new Cloudinary(builder.Configuration.GetConnectionString("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;

            builder.Services.AddSingleton<Cloudinary>(cloudinary);

            builder.Services.AddScoped<UploadImageService>();

            builder.Services.AddMapster();

            builder.Services.AddSwaggerGen();

            MapsterConfig.Configure();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint(); 
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }

    public static class MapsterConfig
    {
        public static void Configure()
        {
            TypeAdapterConfig<Brand,BrandViewModel>.NewConfig();
            TypeAdapterConfig<Category,CategoryViewModel>.NewConfig();
            TypeAdapterConfig<ProductVariant,ProductVariantViewModel>.NewConfig();
            TypeAdapterConfig<IdentityRole,IdentityRoleViewModel>.NewConfig();
            TypeAdapterConfig<Product, ProductViewModel>.NewConfig()
                .Map(dest => dest.Categories, src => src.Categories.Select(x => x.Name))
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
        }
    }
}
