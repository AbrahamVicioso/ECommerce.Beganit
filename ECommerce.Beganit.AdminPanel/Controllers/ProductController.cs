using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    public class ProductController : Controller
    {
        private readonly ECommerceDBContext eCommerceDBContext;
 
        public ProductController(
            ECommerceDBContext eCommerceDBContext)
        {
            this.eCommerceDBContext = eCommerceDBContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateProduct() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            eCommerceDBContext.Products.Add(product);
            eCommerceDBContext.SaveChanges();
            return View();
        }
    }
}
