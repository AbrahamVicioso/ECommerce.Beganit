using CloudinaryDotNet.Actions;
using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using ECommerce.Beganit.AdminPanel.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    [Authorize(Roles="Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProductController : Controller
    {
        private readonly ECommerceDBContext _context;
        private readonly UploadImageService _uploadImageService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            ECommerceDBContext context,
            UploadImageService uploadImageService,
            IMapper mapper,
            ILogger<ProductController> logger)
        {
            _context = context;
            _uploadImageService = uploadImageService;
            this._mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.ProductImages)
                    .AsNoTracking()
                    .ToListAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product list");
                return View("Error");
            }
        }

        [HttpGet("[controller]/[action]/{id:int}")]
        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null
                ? NotFound()
                : View(product);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewData["Brands"] = await _context.Brands.ToListAsync();
            return View(new ProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    ViewData["Brands"] = await _context.Brands.ToListAsync();
            //    return View(model);
            //}

            try
            {
                Product product = new Product();
                _mapper.Map(model, product);
                if (model.Attributes != null) {
                    foreach (var item in model.Attributes)
                    {
                        product.ProductAttributes.Add(new ProductAttribute()
                        {
                            AttributeName = item.AttributeName,
                            AttributeValue = item.AttributeValue
                        });
                    }
                }
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the product.");
                ViewData["Brands"] = await _context.Brands.ToListAsync();
                return View(model);
            }
        }

        [HttpGet("[controller]/[action]/{id:int}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
                .Include(x => x.ProductAttributes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();


            ViewData["AllCategories"] = await _context.Categories.AsNoTracking().ToListAsync();
            ViewData["Brands"] = await _context.Brands.AsNoTracking().ToListAsync();
            ViewData["Product"] = product;

            var model = _mapper.Map<ProductViewModel>(product);
            model.Categories = product.Categories.Select(x => x.Name).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            Product product = _context.Products
                .Include(x => x.Categories)
                .Include(x => x.ProductAttributes)
                .Where(x => x.Id == model.Id).FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            _mapper.Map(model,product);

            if (product.Categories.Count() > 0)
            {
                product.Categories.Clear();
            }

            if (model.Categories != null)
            {
                foreach(string category in model.Categories)
                {
                    Category categoryCurrent = _context.Categories.FirstOrDefault(x => x.Name == category);
                    if (categoryCurrent != null)
                    {
                        product.Categories.Add(categoryCurrent);
                    }
                }
            }

            if (product.ProductAttributes != null)
            {
                product.ProductAttributes.Clear();
            }

            if (model.Attributes != null)
            {
                foreach (var attribute in model.Attributes)
                {
                    product.ProductAttributes.Add(new ProductAttribute()
                    {
                        AttributeName = attribute.AttributeName,
                        AttributeValue = attribute.AttributeValue,
                        ProductId = product.Id,
                    });
                }
            }

            _context.Products.Update(product);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[controller]/[action]/{id}")]
        public IActionResult DisableProduct(int id)
        {
            Product? product = _context.Products
                .Include(x => x.ProductImages)
                .FirstOrDefault(x => x.Id == id);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DisableProduct(Product? product)
        {
            product = _context.Products.FirstOrDefault(x => x.Id == product.Id);
            if (product == null)
            {
                return View("Error");
            }
            product.IsActive = false;
            _context.Update(product);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[controller]/[action]/{productId:int}")]
        public async Task<IActionResult> UploadImage(int productId)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound();

            var model = new UploadProductImageViewModel
            {
                ProductId = productId,
                DisplayOrder = product.ProductImages.Count + 1
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile Image,UploadProductImageViewModel model)
        {
            IEnumerable<ProductImage> images = _context.ProductImages.Where(x => x.ProductId == model.ProductId);

            ImageUploadResult result = await _uploadImageService.Upload(Image);

            _context.ProductImages.Add(new ProductImage()
            {
                ProductId = model.ProductId,
                AltText = model.AltText,
                DisplayOrder = model.DisplayOrder,
                ImageUrl = "https://res.cloudinary.com/dppajuos8/image/upload/v1742961023/" + result.PublicId,
                IsPrimary = model.IsPrimary
            });

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[controller]/[action]/{imageId}")]
        public IActionResult EditImage(int imageId)
        {
            ProductImage image = _context.ProductImages.Where(x => x.Id == imageId).First();
            return View(new UploadProductImageViewModel()
            {
                ImageId = image.Id,
                ImageUrl = image.ImageUrl,
                DisplayOrder = image.DisplayOrder ?? 0,
                AltText = image.AltText,
                IsPrimary = image.IsPrimary ?? false,
                ProductId = image.ProductId ?? 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage([FromForm] IFormFile Image, UploadProductImageViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _uploadImageService.Upload(Image);

                var productImage = _context.ProductImages.Where(x => x.Id == model.ImageId).First();
                productImage.ProductId = model.ProductId;
                productImage.AltText = model.AltText;
                productImage.DisplayOrder = model.DisplayOrder;
                productImage.ImageUrl = $"https://res.cloudinary.com/dppajuos8/image/upload/v1742961023/{result.PublicId}";
                productImage.IsPrimary = model.IsPrimary;
                _context.Update(productImage);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Image uploaded successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image");
                ModelState.AddModelError(string.Empty, "An error occurred while uploading the image.");
                return View(model);
            }
        }

        private bool ProductExists(int id) =>
            _context.Products.Any(p => p.Id == id);
    }
}


//using ECommerce.Beganit.AdminPanel.Data;
//using ECommerce.Beganit.AdminPanel.Models;
//using ECommerce.Beganit.AdminPanel.Models.ViewModels;
//using ECommerce.Beganit.AdminPanel.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.CodeAnalysis;
//using Microsoft.EntityFrameworkCore;

//namespace ECommerce.Beganit.AdminPanel.Controllers
//{
//    public class ProductController : Controller
//    {
//        private readonly _context _context;

//        private readonly UploadImageService uploadImageService;

//        public ProductController(
//            _context _context,
//            UploadImageService uploadImageService
//            )
//        {
//            this._context = _context;
//            this.uploadImageService = uploadImageService;
//        }

//        public IActionResult Index()
//        {
//            return View(_context.Products.Include(x => x.ProductImages));
//        }

//        [HttpGet("[controller]/[action]/{id}")]
//        public IActionResult ProductDetail(int id)
//        {
//            Product? product = _context.Products
//                .Include(x => x.ProductImages)
//                .First(x => x.Id == id);
//            return View(product);
//        }

//        public IActionResult CreateProduct()
//        {
//            ViewData["Brands"] = _context.Brands;
//            return View();
//        }

//        [HttpPost]
//        public IActionResult CreateProduct(Product product)
//        {
//            _context.Products.Add(product);
//            _context.SaveChanges();
//            return RedirectToActionPermanent(nameof(Index));
//            //PREGUNTAR DIFERENCIAS DE NORMAL AND PERMANENT
//        }

//        [HttpGet("[controller]/[action]/{id}")]
//        public IActionResult EditProduct(int id)
//        {
//            List<Brand> brands = _context.Brands.ToList();
//            Product? product = _context.Products.Include(x => x.ProductImages).FirstOrDefault(x => x.Id == id);
//            ViewData["Product"] = product;
//            ViewData["Brands"] = brands;
//            return View(product);
//        }

//        [HttpPost]
//        public IActionResult EditProduct(Product product)
//        {
//            _context.Update(product);
//            _context.SaveChanges();
//            return RedirectToActionPermanent(nameof(Index));

//        }

//        [HttpGet("[controller]/[action]/{id}")]
//        public IActionResult DisableProduct(int id)
//        {
//            Product? product = _context.Products.FirstOrDefault(x => x.Id == id);
//            return View(product);
//        }

//        [HttpPost]
//        public IActionResult DisableProduct(Product? product)
//        {
//            product = _context.Products.FirstOrDefault(x => x.Id == product.Id);
//            if (product == null)
//            {
//                return View("Error");
//            }
//            product.IsActive = false;
//            _context.Update(product);
//            _context.SaveChanges();

//            return RedirectToAction(nameof(Index));
//        }

//        [HttpGet("[controller]/[action]/{productId}")]
//        public IActionResult UploadImage(int productId, int ord)
//        {
//            Product product = _context.Products.Include(x => x.ProductImages).FirstOrDefault(x => x.Id == productId);

//            ViewData["Product"] = product;

//            UploadProductImageViewModel model = new UploadProductImageViewModel()
//            {
//                ProductId = productId,
//                DisplayOrder = product.ProductImages.Count + 1
//            };
//            return View(model);
//        }

//        [HttpPost]
//        public async Task<IActionResult> UploadImage(UploadProductImageViewModel model)
//        {
//            IEnumerable<ProductImage> images = _context.ProductImages.Where(x => x.ProductId == model.ProductId);

//            ImageUploadResult result = await uploadImageService.Upload(model.Image);

//            _context.ProductImages.Add(new ProductImage()
//            {
//                ProductId = model.ProductId,
//                AltText = model.AltText,
//                DisplayOrder = model.DisplayOrder,
//                ImageUrl = "https://res.cloudinary.com/dppajuos8/image/upload/v1742961023/" + result.PublicId,
//                IsPrimary = model.IsPrimary
//            });

//            _context.SaveChanges();

//            return RedirectToAction(nameof(Index));
//        }

//        [HttpGet("[controller]/[action]/{imageId}")]
//        public IActionResult EditImage(int imageId)
//        {
//            ProductImage image = _context.ProductImages.Where(x => x.Id == imageId).First();
//            return View(new UploadProductImageViewModel()
//            {
//                ImageId = image.Id,
//                ImageUrl = image.ImageUrl,
//                DisplayOrder = image.DisplayOrder ?? 0,
//                AltText = image.AltText,
//                IsPrimary = image.IsPrimary ?? false,
//                ProductId = image.ProductId ?? 0
//            });
//        }

//        [HttpPost]
//        public async Task<IActionResult> EditImage(UploadProductImageViewModel model)
//        {
//            ImageUploadResult result = await uploadImageService.Upload(model.Image);

//            ProductImage image = _context.ProductImages.Where(x => x.Id == model.ImageId).First();

//            image.ImageUrl = "https://res.cloudinary.com/dppajuos8/image/upload/v1742961023/" + result.PublicId;
//            image.DisplayOrder = model.DisplayOrder;
//            image.AltText = model.AltText;
//            image.IsPrimary = model.IsPrimary;
//            image.ProductId = model.ProductId;


//            _context.Update(image);

//            _context.SaveChanges();

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}



