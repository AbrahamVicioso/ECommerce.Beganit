using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using ECommerce.Beganit.AdminPanel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.Beganit.AdminPanel.Data;
using MapsterMapper;

namespace ECommerce.Beganit.AdminPanel.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public CategoryController(ECommerceDBContext eCommerceDBContext, IMapper mapper)
        {
            this._context = eCommerceDBContext;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CategoryViewModel>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            var totalCount = await _context.Categories.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            var categories = await _context.Categories
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
