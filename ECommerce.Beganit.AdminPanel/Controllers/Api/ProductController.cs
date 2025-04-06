using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Beganit.AdminPanel.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ProductController : ControllerBase
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public ProductController(ECommerceDBContext eCommerce, IMapper mapper)
        {
            this._context = eCommerce;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<ProductViewModel>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            var totalCount = await _context.Products.Where(x => x.IsActive).CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            var products = await _context.Products
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
                .Include(p => p.Reviews.Where(x => x.IsApproved == true))
                .Include(p => p.ProductAttributes)
                .Include(p => p.ProductVariants)
                .Where(p => p.IsActive)
                .Select(x => _mapper.Map<ProductViewModel>(x))
                .ToListAsync();

            var response = new PaginatedResponse<ProductViewModel>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = parameters.Page,
                PageSize = parameters.PageSize,
                Items = products
            };

            return Ok(response);
        }

        [HttpGet("GetProduct")]
        public async Task<ActionResult<ProductViewModel>> GetProduct(string slug)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductAttributes)
                .Include(p => p.Categories)
                .Include(p => p.Reviews)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantAttributes)
                .Where(p => p.IsActive)
                .Where(p => p.Slug == slug)
                .FirstOrDefaultAsync();


            if (product == null)
            {
                return NotFound();
            }

            var productViewModel = _mapper.Map<ProductViewModel>(product);
            return Ok(productViewModel);
        }
    }
}
