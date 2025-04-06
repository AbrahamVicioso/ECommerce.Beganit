using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Beganit.AdminPanel.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class BundleProductController : ControllerBase
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public BundleProductController(ECommerceDBContext eCommerce, IMapper mapper)
        {
            this._context = eCommerce;
            this._mapper = mapper;
        }

        [HttpGet]
        public  async Task<ActionResult<PaginatedResponse<ProductBundleViewModel>>> GetAll([FromQuery] PaginationParameters parameters) {
            var totalCount = await _context.ProductBundles.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            var bundles = await _context.ProductBundles
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Include(x => x.ProductBundleItems)
                .Where(x => x.IsActive?? false)
                .Select(x => _mapper.Map<ProductBundleViewModel>(x))
                .ToListAsync();

            var response = new PaginatedResponse<ProductBundleViewModel>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = parameters.Page,
                PageSize = parameters.PageSize,
                Items = bundles
            };

            return Ok(response);
        }
    }
}
