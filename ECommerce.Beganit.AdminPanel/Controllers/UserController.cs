using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Email,PhoneNumber")] IdentityUser user, string password)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Obtener los roles del usuario
            var userRoles = await _userManager.GetRolesAsync(user);

            // Obtener todos los roles disponibles
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            // Filtrar los roles que el usuario no tiene aún
            var availableRoles = allRoles.Except(userRoles).ToList();

            var viewModel = new UserRolesViewModel
            {
                User = user,
                UserRoles = userRoles.ToList(),
                AvailableRoles = availableRoles
            };

            return View(viewModel);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email,PhoneNumber,EmailConfirmed,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,LockoutEnd,SecurityStamp,ConcurrencyStamp,PasswordHash,NormalizedUserName,NormalizedEmail,AccessFailedCount")] IdentityUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _userManager.FindByIdAsync(id);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.UserName = user.UserName;
                    existingUser.Email = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.EmailConfirmed = user.EmailConfirmed;
                    existingUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                    existingUser.TwoFactorEnabled = user.TwoFactorEnabled;
                    existingUser.LockoutEnabled = user.LockoutEnabled;
                    existingUser.LockoutEnd = user.LockoutEnd;

                    var result = await _userManager.UpdateAsync(existingUser);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Si llegamos aquí, algo falló - preparamos el ViewModel de nuevo
            var userRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id));
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var availableRoles = allRoles.Except(userRoles).ToList();

            var viewModel = new UserRolesViewModel
            {
                User = user,
                UserRoles = userRoles.ToList(),
                AvailableRoles = availableRoles
            };

            return View(viewModel);
        }

        // POST: Users/AddUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Verificar si el rol existe
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                ModelState.AddModelError("", $"El rol {role} no existe.");
                return RedirectToAction(nameof(Edit), new { id = userId });
            }

            // Verificar si el usuario ya tiene el rol
            var userHasRole = await _userManager.IsInRoleAsync(user, role);
            if (userHasRole)
            {
                ModelState.AddModelError("", $"El usuario ya tiene el rol {role}.");
                return RedirectToAction(nameof(Edit), new { id = userId });
            }

            // Añadir el rol al usuario
            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Edit), new { id = userId });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction(nameof(Edit), new { id = userId });
        }

        // POST: Users/RemoveUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Verificar si el usuario tiene el rol
            var userHasRole = await _userManager.IsInRoleAsync(user, role);
            if (!userHasRole)
            {
                ModelState.AddModelError("", $"El usuario no tiene el rol {role}.");
                return RedirectToAction(nameof(Edit), new { id = userId });
            }

            // Quitar el rol del usuario
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Edit), new { id = userId });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction(nameof(Edit), new { id = userId });
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return View(user);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UserExists(string id)
        {
            return await _userManager.Users.AnyAsync(e => e.Id == id);
        }
    }
}
