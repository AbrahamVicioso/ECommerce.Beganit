using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Beganit.AdminPanel.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public ReviewController(ECommerceDBContext eCommerceDBContext, IMapper mapper)
        {
            this._context = eCommerceDBContext;
            this._mapper = mapper;
        }

        // Obtener el ID del usuario autenticado
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null)
            {
                var customer = _context.Customers.FirstOrDefault(x => x.UserId == userIdClaim);
                if (customer != null)
                {
                    return customer.Id;
                }
            }
            return 0;
        }

        // GET: api/Review/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewViewModel>>> GetReviewsByProduct(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved == true)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ReviewViewModel>>(reviews);
        }

        // POST: api/Review
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ReviewViewModel>> CreateReview(ReviewViewModel reviewViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            var review = _mapper.Map<Review>(reviewViewModel);
            review.CustomerId = userId; // Asignar el ID del cliente autenticado
            review.CreatedAt = DateTime.UtcNow;
            review.IsApproved = false; // Por defecto, las reviews necesitan aprobación

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            reviewViewModel = _mapper.Map<ReviewViewModel>(review);

            return CreatedAtAction(nameof(GetReviewsByProduct), new { productId = reviewViewModel.ProductId }, reviewViewModel);
        }
    }
}