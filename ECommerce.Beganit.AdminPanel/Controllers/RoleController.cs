using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMapper _mapper;

        public RoleController(RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            this.roleManager = roleManager;
            this._mapper = mapper;
        }

        public IActionResult Index()
        {
            return View(roleManager.Roles.Select(x => _mapper.Map<IdentityRoleViewModel>(x)));
        }

        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IdentityRoleViewModel model)
        {
            await roleManager.CreateAsync(new IdentityRole()
            {
                ConcurrencyStamp = model.ConcurrencyStamp,
                Name = model.Name,
                NormalizedName = model.NormalizedName
            });
            return RedirectToAction(nameof(Index));
        }
    }
}
