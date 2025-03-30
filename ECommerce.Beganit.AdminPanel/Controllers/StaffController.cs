using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class StaffController : Controller
    {
        private readonly ECommerceDBContext eCommerceDBContext;

        public StaffController(ECommerceDBContext eCommerceDBContext)
        {
            this.eCommerceDBContext = eCommerceDBContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateStaff()
        {
            ViewData["Users"] = eCommerceDBContext.Users.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult CreateStaff(StaffViewModelBase staff) 
        {
            ViewData["Users"] = eCommerceDBContext.Users.ToList();

            eCommerceDBContext.Staff.Add(new()
            {
                UserId = staff.UserId
            });

            eCommerceDBContext.SaveChanges();
            return View();
        }

        public IActionResult EditStaff() 
        {
            ViewData["Users"] = eCommerceDBContext.Users.ToList();
            return View();
        }

    }
}
