using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Beganit.AdminPanel.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTest()
        {
            return Ok(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
