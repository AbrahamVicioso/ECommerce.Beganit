using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Beganit_AspMVC_Administrator.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateProduct()
        {
            return View();
        }
    }
}
