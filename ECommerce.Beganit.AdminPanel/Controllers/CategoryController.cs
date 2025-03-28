using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using ECommerce.Beganit.AdminPanel.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ECommerceDBContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly UploadImageService _uploadImageService;

        public CategoryController(ECommerceDBContext commerceDBContext,
            Cloudinary cloudinary,
            UploadImageService uploadImageService
            )
        {
            this._context = commerceDBContext;
            this._cloudinary = cloudinary;
            this._uploadImageService = uploadImageService;
        }

        public ActionResult Index()
        {
            // Fetch all categories from the database
            var categories = _context.Categories.ToList()
                .Select(x => new CategoryViewModel
                {
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    Description = x.Description,
                    DisplayOrder = x.DisplayOrder,
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    ParentId = x.ParentId
                })
                .ToList();

            // Client-side filtering
            var rootCategories = categories
                .Where(c => c.ParentId == null || c.ParentId == 0)
                .ToList();

            var childCategories = categories
                .Where(c => c.ParentId != null && c.ParentId > 0)
                .GroupBy(c => c.ParentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.ChildCategories = childCategories;

            return View(rootCategories);
        }

        public IActionResult Create() {
            ViewData["Categories"] = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            ViewData["Categories"] = _context.Categories.ToList() ?? new List<Category>();

            if (!ModelState.IsValid)
            {
                ImageUploadResult result =  await _uploadImageService.Upload(model.Image);

                Category category = new()
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    CreatedBy = model.CreatedBy,
                    DisplayOrder = model.DisplayOrder,
                    Id = model.Id,
                    ImageUrl = "https://res.cloudinary.com/dppajuos8/image/upload/v1742961023/" + result.PublicId,
                    Slug = model.Slug,
                    IsActive = model.IsActive,
                    ParentId = model.ParentId,
                };

                _context.Categories.Add(category);

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet("[controller]/[action]/{id}")]
        public IActionResult Edit(int id)
        {
            // Obtiene la categoría actual
            var currentCategory = _context.Categories.Find(id);

            if (currentCategory == null)
            {
                return NotFound();
            }

            // Filtra las categorías para excluir:
            // 1. La categoría actual
            // 2. Las subcategorías de la categoría actual (para evitar ciclos)
            ViewBag.Categories = _context.Categories
                .Where(c =>
                    c.Id != id && // Excluye la categoría actual
                    !_context.Categories.Any(sub => sub.ParentId == id && sub.Id == c.Id) // Excluye subcategorías
                )
                .ToList();

            CategoryViewModel category = new CategoryViewModel
            {
                Id = currentCategory.Id,
                Name = currentCategory.Name,
                Description = currentCategory.Description,
                CreatedAt = currentCategory.CreatedAt,
                CreatedBy = currentCategory.CreatedBy,
                DisplayOrder = currentCategory.DisplayOrder,
                ImageUrl = currentCategory.ImageUrl,
                Slug = currentCategory.Slug,
                IsActive = currentCategory.IsActive ?? false,
                ParentId = currentCategory.ParentId,
                UpdatedAt = currentCategory.UpdatedAt,
                UpdatedBy = currentCategory.UpdatedBy,
            };

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            // Obtener las categorías para el dropdown
            ViewData["Categories"] = _context.Categories.ToList() ?? new List<Category>();

            // Validar que la categoría no sea su propio padre
            if (model.Id == model.ParentId)
            {
                ModelState.AddModelError("ParentId", "A category cannot be its own parent.");
                return View(model);
            }

            // Buscar la categoría existente
            Category? category = await _context.Categories.FindAsync(model.Id);
            if (category == null)
            {
                return NotFound();
            }

            // Manejar la carga de imagen
            ImageUploadResult? result = null;
            if (model.Image != null)
            {
                result = await _uploadImageService.Upload(model.Image);
            }

            // Actualizar propiedades de la categoría
            category.Name = model.Name;
            category.Description = model.Description;

            // Mantener la fecha de creación original
            // category.CreatedAt no se modifica

            category.CreatedBy = model.CreatedBy;
            category.DisplayOrder = model.DisplayOrder;

            // Actualizar URL de imagen solo si se sube una nueva
            if (result != null)
            {
                category.ImageUrl = "https://res.cloudinary.com/dppajuos8/image/upload/v1742961023/" + result.PublicId;
            }
            // Si no se sube imagen nueva, mantener la imagen existente

            category.Slug = model.Slug;
            category.IsActive = model.IsActive;

            // Verificar nuevamente que no sea su propio padre
            if (model.ParentId == category.Id)
            {
                ModelState.AddModelError("ParentId", "A category cannot be its own parent.");
                return View(model);
            }
            category.ParentId = model.ParentId;

            // Actualizar la fecha de modificación
            category.UpdatedAt = DateTime.UtcNow;
            //category.UpdatedBy = User.Identity?.Name; // Asumiendo que tienes autenticación

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Manejar posibles errores de actualización
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category.");
                return View(model);
            }
        }
    }
}
