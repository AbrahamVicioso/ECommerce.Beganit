using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProductVariantController : Controller
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductVariantViewModel> _logger;

        public ProductVariantController(
            ECommerceDBContext eCommerceDBContext,
            IMapper mapper,
            ILogger<ProductVariantViewModel> logger
        )
        {
            this._context = eCommerceDBContext;
            this._mapper = mapper;
            this._logger = logger;
        }

        public IActionResult Index()
        {
            // Fetch product variants
            var productVariants = _context.ProductVariants.Select(x => _mapper.Map<ProductVariantViewModel>(x)).ToList();

            // Fetch all products to associate with variants
            var products = _context.Products.Include(x => x.ProductImages).ToList();

            // Pass products to the view via ViewData
            ViewData["Products"] = products;

            return View(productVariants);
        }

        [HttpGet("[controller]/[action]/{id}")]
        public IActionResult Details(int id)
        {
            ProductVariantViewModel model = _context.ProductVariants
                .Where(x => x.Id == id)
                .Select(x => _mapper.Map<ProductVariantViewModel>(x))
                .FirstOrDefault();

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            ViewData["Products"] = _context.Products;
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductVariantViewModel model)
        {
            _context.ProductVariants.Add(_mapper.Map<ProductVariant>(model));
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        [HttpGet("[controller]/[action]/{id}")]
        public IActionResult Edit(int id) {
            ViewData["Products"] = _context.Products.ToList();
            ProductVariantViewModel model = _context.ProductVariants
                .Where(x => x.Id == id)
                .Select(x => _mapper.Map<ProductVariantViewModel>(x))
                .FirstOrDefault();

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductVariantViewModel model)
        {
            // Check if the model is null
            if (model == null)
            {
                return BadRequest("Invalid product variant data.");
            }

            // Find the existing product variant
            var productCurrent = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (productCurrent == null)
            {
                return NotFound($"Product variant with ID {model.Id} not found.");
            }

            // Validate the model state
            if (!ModelState.IsValid)
            {
                // Log model state errors if needed
                return View(model);
            }

            try
            {
                // Use AutoMapper to map the view model to the existing entity
                _mapper.Map(model, productCurrent);

                // Detach any tracked entities with the same ID to prevent multiple tracking
                var trackedEntity = _context.ProductVariants.Local
                    .FirstOrDefault(x => x.Id == model.Id);

                if (trackedEntity != null)
                {
                    _context.Entry(trackedEntity).State = EntityState.Detached;
                }

                // Update the entity
                _context.ProductVariants.Update(productCurrent);

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Add a temporary success message
                TempData["SuccessMessage"] = "Product variant updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflicts
                ModelState.AddModelError(string.Empty, "The record you attempted to edit was modified by another user.");
                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error updating product variant");

                // Add a generic error message
                ModelState.AddModelError(string.Empty, "An error occurred while updating the product variant.");
                return View(model);
            }
        }
    }
}
