using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    public class BundleController : Controller
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public BundleController(ECommerceDBContext eCommerce, IMapper mapper)
        {
            this._context = eCommerce;
            this._mapper = mapper;
        }

        [HttpGet]
        public IActionResult SearchProducts(string searchTerm, int? categoryId, int? brandId, int page = 1, int pageSize = 10)
        {
            // Iniciar la consulta
            var query = _context.Products.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Sku.Contains(searchTerm));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.Categories.Contains(new Models.Category()
                {
                    Id = categoryId.Value,
                }));
            }

            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // Calcular total y paginación
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Obtener productos paginados
            var products = query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    sku = p.Sku,
                    price = p.RegularPrice,
                    stockQuantity = p.Quantity,
                    imageUrl = p.ProductImages.Where(p => p.IsPrimary == true).FirstOrDefault(),
                    brandName = p.Brand.Name
                })
                .ToList();

            // Devolver resultados como JSON
            return Json(new
            {
                products,
                totalCount,
                totalPages,
                currentPage = page,
                imageUrl = products.Select(x => x.imageUrl)
            });
        }

        public IActionResult Index()
        {
            // Obtener todos los bundles con sus items relacionados
            var bundles = _context.ProductBundles
                .Include(b => b.ProductBundleItems)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            // Mapear a ViewModels
            var bundleViewModels = bundles.Select(b => {
                var viewModel = _mapper.Map<ProductBundleViewModel>(b);
                viewModel.Items = b.ProductBundleItems.Select(i => _mapper.Map<ProductBundleItemViewModel>(i)).ToList();
                return viewModel;
            }).ToList();

            return View(bundleViewModels);
        }

        public IActionResult Create()
        {
            ViewData["Categories"] = _context.Categories;
            ViewData["Brands"] = _context.Brands;
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductBundleViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create the bundle
                    var bundle = new Models.ProductBundle
                    {
                        Name = model.Name,
                        Description = model.Description,
                        DiscountPercentage = model.DiscountPercentage ?? 0,
                        IsActive = model.IsActive ?? true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Add the bundle to the context
                    _context.ProductBundles.Add(bundle);
                    _context.SaveChanges();

                    // Add bundle items if they exist
                    if (model.Items != null && model.Items.Count > 0)
                    {
                        foreach (var item in model.Items)
                        {
                            var bundleItem = new Models.ProductBundleItem
                            {
                                BundleId = bundle.Id,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity
                            };

                            _context.ProductBundleItems.Add(bundleItem);
                        }

                        // Save the bundle items
                        _context.SaveChanges();
                    }

                    // Redirect to the index page after successful creation
                    TempData["SuccessMessage"] = "Bundle created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the error
                    ModelState.AddModelError("", "Failed to create bundle: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewData["Categories"] = _context.Categories;
            ViewData["Brands"] = _context.Brands;
            return View(model);
        }

        // GET: Bundle/Edit/5
        public IActionResult Edit(int id)
        {
            // Get the bundle with its items
            var bundle = _context.ProductBundles
                .Include(b => b.ProductBundleItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(b => b.Id == id);

            if (bundle == null)
            {
                return NotFound();
            }

            // Map to view model
            var viewModel = _mapper.Map<ProductBundleViewModel>(bundle);
            viewModel.Items = bundle.ProductBundleItems.Select(i => _mapper.Map<ProductBundleItemViewModel>(i)).ToList();

            // Add categories and brands to ViewData for dropdowns
            ViewData["Categories"] = _context.Categories;
            ViewData["Brands"] = _context.Brands;

            return View(viewModel);
        }

        // POST: Bundle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProductBundleViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing bundle
                    var bundle = _context.ProductBundles
                        .Include(b => b.ProductBundleItems)
                        .FirstOrDefault(b => b.Id == id);

                    if (bundle == null)
                    {
                        return NotFound();
                    }

                    // Update bundle properties
                    bundle.Name = model.Name;
                    bundle.Description = model.Description;
                    bundle.DiscountPercentage = model.DiscountPercentage ?? 0;
                    bundle.IsActive = model.IsActive ?? true;
                    bundle.UpdatedAt = DateTime.Now;

                    // Remove existing bundle items
                    _context.ProductBundleItems.RemoveRange(bundle.ProductBundleItems);

                    // Add updated bundle items
                    if (model.Items != null && model.Items.Count > 0)
                    {
                        foreach (var item in model.Items)
                        {
                            var bundleItem = new Models.ProductBundleItem
                            {
                                BundleId = bundle.Id,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity
                            };

                            _context.ProductBundleItems.Add(bundleItem);
                        }
                    }

                    // Save changes
                    _context.Update(bundle);
                    _context.SaveChanges();

                    // Success message
                    TempData["SuccessMessage"] = "Bundle updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BundleExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    ModelState.AddModelError("", "Failed to update bundle: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewData["Categories"] = _context.Categories;
            ViewData["Brands"] = _context.Brands;
            return View(model);
        }

        // Helper method to check if bundle exists
        private bool BundleExists(int id)
        {
            return _context.ProductBundles.Any(e => e.Id == id);
        }
    }
}
