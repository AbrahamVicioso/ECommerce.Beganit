using CloudinaryDotNet.Actions;
using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using ECommerce.Beganit.AdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    public class BrandController : Controller
    {
        private readonly UploadImageService _uploadImageService;

        public ECommerceDBContext _context { get; }

        public BrandController(
            UploadImageService uploadImageService,
            ECommerceDBContext eCommerceDBContext
            )
        {
            this._uploadImageService = uploadImageService;
            _context = eCommerceDBContext;
        }

        public IActionResult Index()
        {
            return View(_context.Brands.Select(x => new BrandViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                LogoUrl = x.LogoUrl,
                UpdatedAt = x.UpdatedAt,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                Description = x.Description,
                UpdatedBy = x.UpdatedBy,
            }));
        }

        public IActionResult Create()
        {
            return View(new BrandViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandViewModel model)
        {
            ImageUploadResult result = await _uploadImageService.Upload(model.Image);

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
        public IActionResult Delete(int id)
        {

            return View(_context.Brands.Where(x => x.Id == id).Select(x => new BrandViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                LogoUrl = x.LogoUrl,
                UpdatedAt = x.UpdatedAt,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                Description = x.Description,
                UpdatedBy = x.UpdatedBy,
            }).First());
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
    }
}
