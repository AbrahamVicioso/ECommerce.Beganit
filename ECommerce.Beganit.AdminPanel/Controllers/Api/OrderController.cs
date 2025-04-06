using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Beganit.AdminPanel.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public OrderController(ECommerceDBContext eCommerceDBContext, IMapper mapper)
        {
            this._context = eCommerceDBContext;
            this._mapper = mapper;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderViewModel>>> GetOrders(
            [FromQuery] int? customerId,
            [FromQuery] int? statusId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Orders.AsQueryable();

            // Apply filters
            if (customerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == customerId.Value);
            }

            if (statusId.HasValue)
            {
                query = query.Where(o => o.OrderStatusId == statusId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderViewModels = _mapper.Map<List<OrderViewModel>>(orders);

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Total-Pages", totalPages.ToString());

            return Ok(orderViewModels);
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderViewModel>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderStatusHistories)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderViewModel = _mapper.Map<OrderViewModel>(order);
            return orderViewModel;
        }

        // GET: api/Order/number/ORD12345
        [HttpGet("number/{orderNumber}")]
        public async Task<ActionResult<OrderViewModel>> GetOrderByNumber(string orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderStatusHistories)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null)
            {
                return NotFound();
            }

            var orderViewModel = _mapper.Map<OrderViewModel>(order);
            return orderViewModel;
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<OrderViewModel>> CreateOrder(OrderViewModel orderViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generate order number if not provided
            if (string.IsNullOrEmpty(orderViewModel.OrderNumber))
            {
                orderViewModel.OrderNumber = GenerateOrderNumber();
            }

            // Set dates if not provided
            if (!orderViewModel.OrderDate.HasValue)
            {
                orderViewModel.OrderDate = DateTime.UtcNow;
            }

            orderViewModel.CreatedAt = DateTime.UtcNow;
            orderViewModel.UpdatedAt = DateTime.UtcNow;

            // Map to entity and save
            var order = _mapper.Map<Order>(orderViewModel);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create initial order status history
            if (orderViewModel.OrderStatusId.HasValue)
            {
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    OrderStatusId = orderViewModel.OrderStatusId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1 // Default admin user ID or get from authentication
                };

                _context.OrderStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, _mapper.Map<OrderViewModel>(order));
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, OrderViewModel orderViewModel)
        {
            if (id != orderViewModel.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if order exists
            var orderExists = await _context.Orders.AnyAsync(o => o.Id == id);
            if (!orderExists)
            {
                return NotFound();
            }

            // Update modification date
            orderViewModel.UpdatedAt = DateTime.UtcNow;

            // Map and update
            var order = _mapper.Map<Order>(orderViewModel);
            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Orders.AnyAsync(o => o.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Instead of hard delete, consider setting a flag or moving to archive
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Order/5/status/2
        [HttpPut("{id}/status/{statusId}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, int statusId)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var status = await _context.OrderStatuses.FindAsync(statusId);
            if (status == null)
            {
                return BadRequest("Invalid status ID");
            }

            // Update order status
            order.OrderStatusId = statusId;
            order.UpdatedAt = DateTime.UtcNow;
            _context.Entry(order).State = EntityState.Modified;

            // Add status history entry
            var statusHistory = new OrderStatusHistory
            {
                OrderId = id,
                OrderStatusId = statusId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1 // Default admin user ID or get from authentication
            };

            _context.OrderStatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Order/5/items
        [HttpGet("{id}/items")]
        public async Task<ActionResult<IEnumerable<OrderItemViewModel>>> GetOrderItems(int id)
        {
            var orderItems = await _context.OrderItems
                .Where(i => i.OrderId == id)
                .ToListAsync();

            if (orderItems == null || !orderItems.Any())
            {
                return NotFound();
            }

            return Ok(_mapper.Map<List<OrderItemViewModel>>(orderItems));
        }

        // POST: api/Order/5/items
        [HttpPost("{id}/items")]
        public async Task<ActionResult<OrderItemViewModel>> AddOrderItem(int id, OrderItemViewModel itemViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            itemViewModel.OrderId = id;

            // Calculate total price
            itemViewModel.TotalPrice = itemViewModel.UnitPrice * itemViewModel.Quantity;

            var orderItem = _mapper.Map<OrderItem>(itemViewModel);
            _context.OrderItems.Add(orderItem);

            // Update order totals
            await UpdateOrderTotals(id);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderItems), new { id = id }, _mapper.Map<OrderItemViewModel>(orderItem));
        }

        // PUT: api/Order/5/items/3
        [HttpPut("{orderId}/items/{itemId}")]
        public async Task<IActionResult> UpdateOrderItem(int orderId, int itemId, OrderItemViewModel itemViewModel)
        {
            if (itemId != itemViewModel.Id || orderId != itemViewModel.OrderId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Calculate total price
            itemViewModel.TotalPrice = itemViewModel.UnitPrice * itemViewModel.Quantity;

            var orderItem = _mapper.Map<OrderItemViewModel>(itemViewModel);
            _context.Entry(orderItem).State = EntityState.Modified;

            try
            {
                // Update order totals
                await UpdateOrderTotals(orderId);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.OrderItems.AnyAsync(i => i.Id == itemId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Order/5/items/3
        [HttpDelete("{orderId}/items/{itemId}")]
        public async Task<IActionResult> DeleteOrderItem(int orderId, int itemId)
        {
            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OrderId == orderId);

            if (orderItem == null)
            {
                return NotFound();
            }

            _context.OrderItems.Remove(orderItem);

            // Update order totals
            await UpdateOrderTotals(orderId);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Order/5/history
        [HttpGet("{id}/history")]
        public async Task<ActionResult<IEnumerable<OrderStatusHistoryViewModel>>> GetOrderStatusHistory(int id)
        {
            var history = await _context.OrderStatusHistories
                .Where(h => h.OrderId == id)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            if (history == null || !history.Any())
            {
                return NotFound();
            }

            return Ok(_mapper.Map<List<OrderStatusHistoryViewModel>>(history));
        }

        // GET: api/Order/statuses
        [HttpGet("statuses")]
        public async Task<ActionResult<IEnumerable<OrderStatusViewModel>>> GetOrderStatuses()
        {
            var statuses = await _context.OrderStatuses.ToListAsync();
            return Ok(_mapper.Map<List<OrderStatusViewModel>>(statuses));
        }

        // Helper methods
        private string GenerateOrderNumber()
        {
            // Generate a unique order number with prefix ORD and random digits
            string prefix = "ORD";
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string random = new Random().Next(1000, 9999).ToString();

            return $"{prefix}{timestamp}{random}";
        }

        private async Task UpdateOrderTotals(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return;
            }

            // Calculate subtotal from order items
            var orderItems = await _context.OrderItems
                .Where(i => i.OrderId == orderId)
                .ToListAsync();

            order.SubTotal = orderItems.Sum(i => i.TotalPrice);

            // Recalculate total amount
            order.TotalAmount = order.SubTotal + order.ShippingAmount + order.TaxAmount - (order.DiscountAmount ?? 0);
            order.UpdatedAt = DateTime.UtcNow;

            _context.Entry(order).State = EntityState.Modified;
        }
    }
}