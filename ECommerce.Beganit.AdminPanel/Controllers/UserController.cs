using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
