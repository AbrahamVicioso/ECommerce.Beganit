using CloudinaryDotNet.Actions;
using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using ECommerce.Beganit.AdminPanel.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BrandController : Controller
    {
        private readonly UploadImageService _uploadImageService;
        private readonly ILogger<BrandController> _logger;
        private readonly IMapper _mapper;

        public ECommerceDBContext _context { get; }

        public BrandController(
            UploadImageService uploadImageService,
            ECommerceDBContext eCommerceDBContext,
            ILogger<BrandController> logger,
            IMapper mapper
            )
        {
            this._uploadImageService = uploadImageService;
            _context = eCommerceDBContext;
            this._logger = logger;
            this._mapper = mapper;
        }

        public IActionResult Index()
        {
            return View(_context.Brands
                .Select(x => _mapper.Map<BrandViewModel>(x)));
        }

        public IActionResult Create()
        {
            return View(new BrandViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] IFormFile Image, BrandViewModel model)
        {
            ImageUploadResult result = await _uploadImageService.Upload(Image);

            _context.Brands.Add(new Models.Brand()
            {
                Name = model.Name,
                Description = model.Description,
                LogoUrl = "https://res.cloudinary.com/dppajuos8/image/upload/v1742961023/" + result.PublicId
            });

            _context.SaveChanges();

            return RedirectToActionPermanent(nameof(Create));

        }

        [HttpGet("[controller]/[action]/{id}")]
        public IActionResult Details(int id)
        {
            var brand = _context.Brands
            .FirstOrDefault(x => x.Id == id);

            // If no brand is found, return NotFound
            if (brand == null)
            {
                return NotFound();
            }

            // Map the Brand entity to ViewModel after database retrieval
            var brandViewModel = _mapper.Map<BrandViewModel>(brand);

            return View(brandViewModel);
        }

        [HttpGet("[controller]/[action]/{id}")]
        public IActionResult Delete(int id)
        {

            return View(_context.Brands.Where(x => x.Id == id)
                .Select(x => _mapper.Map<BrandViewModel>(x))
                .First());
        }

        [HttpPost]
        public IActionResult Delete(BrandViewModel model)
        {
            Brand brand = _context.Brands.Where(x => x.Id == model.Id).First();

            brand.IsActive = false;

            _context.Update(brand);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[controller]/[action]/{id}")]
        public IActionResult Edit(int id)
        {
            var brand = _context.Brands
            .FirstOrDefault(x => x.Id == id);

            // If no brand is found, return NotFound
            if (brand == null)
            {
                return NotFound();
            }

            // Map the Brand entity to ViewModel after database retrieval
            var brandViewModel = _mapper.Map<BrandViewModel>(brand);

            return View(brandViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] IFormFile Image, BrandViewModel model)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Detach any tracked entities with the same ID
                var trackedEntity = _context.Brands.Local
                    .FirstOrDefault(x => x.Id == model.Id);
                if (trackedEntity != null)
                {
                    _context.Entry(trackedEntity).State = EntityState.Detached;
                }

                // Find the existing brand in the database
                var currentBrand = await _context.Brands
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                // If no brand is found, return NotFound
                if (currentBrand == null)
                {
                    return NotFound($"Brand with ID {model.Id} not found.");
                }

                // Use the mapper to update the existing entity
                var updatedBrand = _mapper.Map<Brand>(model);

                // Manually copy properties to avoid tracking issues
                currentBrand.Name = updatedBrand.Name;
                currentBrand.Description = updatedBrand.Description;
                currentBrand.IsActive = updatedBrand.IsActive;

                // Update audit fields
                currentBrand.UpdatedAt = DateTime.UtcNow;
                //currentBrand.UpdatedBy = GetCurrentUserId();

                // Handle file upload if an image is provided
                if (Image != null)
                {
                    ImageUploadResult result = await _uploadImageService.Upload(Image);
                    currentBrand.LogoUrl = "https://res.cloudinary.com/dppajuos8/image/upload/v1742961023/" + result.PublicId;
                }

                // Explicitly state that this entity should be modified
                _context.Entry(currentBrand).State = EntityState.Modified;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Redirect to index or details page
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log the specific database update exception
                _logger.LogError(ex, "Error updating brand with ID {BrandId}", model.Id);
                ModelState.AddModelError(string.Empty, "A database error occurred while updating the brand.");
                return View(model);
            }
            catch (Exception ex)
            {
                // Log any other unexpected exceptions
                _logger.LogError(ex, "Unexpected error updating brand with ID {BrandId}", model.Id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the brand.");
                return View(model);
            }
        }
    }
}
