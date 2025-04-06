using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Beganit.AdminPanel.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public ReviewController(ECommerceDBContext eCommerceDBContext, IMapper mapper)
        {
            _context = eCommerceDBContext;
            _mapper = mapper;
        }

        // Listado de Reviews
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<object>>> GetReviews([FromQuery] PaginationParameters parameters)
        {
            var totalCount = await _context.Reviews.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .ThenInclude(p => p.ProductImages)
                .OrderBy(r => r.IsApproved)
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Content,
                    r.Rating,
                    r.IsApproved,
                    r.IsVerifiedPurchase,
                    productImageUrl = r.Product.ProductImages.Where(x => x.IsPrimary?? false).First().ImageUrl,
                    CustomerName = r.Customer != null ? _context.Users.Where(x => x.Id == _context.Customers.Where(c => c.Id == r.CustomerId).First().UserId).First().Email : "N/A",
                    ProductName = r.Product != null ? r.Product.Name : "N/A"
                })
                .ToListAsync();

            var response = new PaginatedResponse<object>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = parameters.Page,
                PageSize = parameters.PageSize,
                Items = reviews
            };

            return Ok(response);
        }

        // GET: Editar review
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            return View(review);
        }

        // POST: Editar review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,CustomerId,Rating,Title,Content,IsVerifiedPurchase,IsApproved,CreatedAt,UpdatedAt")] Review review)
        {
            if (id != review.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    review.UpdatedAt = DateTime.Now;
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reviews.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        // POST: Update review approval status
        [HttpPost]
        public async Task<IActionResult> UpdateApproval(int id, [FromBody] ApprovalUpdateModel model)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            review.IsApproved = model.IsApproved;
            review.UpdatedAt = DateTime.Now;

            try
            {
                _context.Update(review);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Review approval status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Model for approval update
        public class ApprovalUpdateModel
        {
            public bool IsApproved { get; set; }
        }
    }
}
