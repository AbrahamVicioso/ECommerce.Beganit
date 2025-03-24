using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Beganit_AspMVC_Administrator.Controllers
{
    public class BrandController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateBrand()
        {
            return Ok();
        } 
    }
}
