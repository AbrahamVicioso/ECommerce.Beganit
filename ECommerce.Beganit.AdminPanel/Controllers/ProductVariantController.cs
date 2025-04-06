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
                .Include(x => x.ProductVariantAttributes)
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
            // Cargar la entidad actual con sus atributos
            var productCurrent = await _context.ProductVariants
                .Include(p => p.ProductVariantAttributes)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (productCurrent == null)
            {
                return NotFound();
            }

            // Mapear solo las propiedades básicas del producto
            _mapper.Map(model, productCurrent);

            // Limpiar los atributos existentes
            if (productCurrent.ProductVariantAttributes != null)
            {
                productCurrent.ProductVariantAttributes.Clear();
            }

            // Agregar los nuevos atributos
            if (model.ProductVariantAttributes != null && model.ProductVariantAttributes.Any())
            {
                foreach (var attr in model.ProductVariantAttributes)
                {
                    productCurrent.ProductVariantAttributes.Add(new ProductVariantAttribute
                    {
                        AttributeName = attr.AttributeName,
                        AttributeValue = attr.AttributeValue
                        // No necesitas asignar VariantId, EF Core lo hará automáticamente
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", "No se pudieron guardar los cambios. " + ex.Message);
                return View(model);
            }
        }
    }
}
