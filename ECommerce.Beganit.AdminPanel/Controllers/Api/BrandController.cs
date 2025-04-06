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
    public class BrandController : ControllerBase
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public BrandController(ECommerceDBContext eCommerceDBContext, IMapper mapper)
        {
            this._context = eCommerceDBContext;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<BrandViewModel>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            var totalCount = await _context.Brands.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            var categories = await _context.Brands
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(x => _mapper.Map<CategoryViewModel>(x))
                .ToListAsync();

            var response = new PaginatedResponse<CategoryViewModel>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = parameters.Page,
                PageSize = parameters.PageSize,
                Items = categories
            };

            return Ok(response);
        }
    }
}
