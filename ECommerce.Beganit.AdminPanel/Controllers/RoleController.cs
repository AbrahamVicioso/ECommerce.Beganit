using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using ECommerce.Beganit.AdminPanel.Data;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    [Authorize(Roles = "Admin")] // Solo permitir acceso a administradores
    public class RoleController : Controller
    {
        private readonly ECommerceDBContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(ECommerceDBContext eCommerceDBContext, RoleManager<IdentityRole> roleManager)
        {
            this._context = eCommerceDBContext;
            this._roleManager = roleManager;
        }


        // GET: Roles
        public ActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IdentityRole role)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    // Verificar si el rol ya existe
                    if (await _roleManager.RoleExistsAsync(role.Name))
                    {
                        ModelState.AddModelError("", "El rol ya existe");
                        return View(role);
                    }

                    // Crear el rol
                    var result = await _roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }

                    // Si hay errores, mostrarlos
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.ToString());
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            try
            {
                var result = _roleManager.DeleteAsync(role);
                if (result.IsCompletedSuccessfully)
                {
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("Delete", role);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View("Delete", role);
            }
        }

        //// GET: Roles/Delete/5
        //public async Task<ActionResult> Delete(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return BadRequest();
        //    }

        //    var role = await _roleManager.FindByIdAsync(id);

        //    if (role == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(role);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}