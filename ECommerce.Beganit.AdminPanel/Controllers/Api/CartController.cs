using ECommerce.Beganit.AdminPanel.Data;
using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Beganit.AdminPanel.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CartController : ControllerBase
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public CartController(ECommerceDBContext eCommerceDBContext, IMapper mapper)
        {
            _context = eCommerceDBContext;
            _mapper = mapper;
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

        // Obtener el carrito actual del usuario o null si no existe
        private async Task<Cart> GetCurrentCartAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return null;
            }

            return await _context.Carts
                .Where(c => c.CustomerId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await GetCurrentCartAsync();

            if (cart == null)
            {
                return NotFound("No se encontró un carrito activo para el usuario.");
            }

            var fullCart = await _context.Carts
                //.Include(c => c.CartItems)
                //.ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var cartViewModel = _mapper.Map<CartViewModel>(fullCart);
            return Ok(cartViewModel);
        }

        // GET: api/Cart/products
        [HttpGet("products")]
        public async Task<ActionResult<CartItemViewModel>> GetCartProducts()
        {
            var cart = await GetCurrentCartAsync();

            if (cart == null)
            {
                return NotFound("No se encontró un carrito activo para el usuario.");
            }

            var cartItems = await _context.CartItems
                .Where(x => x.CartId == cart.Id)
                .Include(x => x.Product)
                .ThenInclude(x => x.ProductImages)
                .Include(x => x.Product)
                .ThenInclude(x => x.ProductAttributes)
                .Include(x => x.Variant)
                .ThenInclude(x => x.ProductVariantAttributes)
                //.Select(x => new CartItemViewModel()
                //{
                //    Id = x.Id,
                //    CartId = cart.Id,
                //    ProductId = x.ProductId,
                //    Quantity = x.Quantity,
                //    Product = _mapper.Map<ProductViewModel>(x.Product)
                //    //Product = new ProductViewModel()
                //    //{
                //    //    Id = x.Product.Id,
                //    //    Name = x.Product.Name,
                //    //    RegularPrice = x.Product.RegularPrice,
                //    //    Images = x.Product.ProductImages.Select(x => _mapper.Map<ProductImageViewModel>(x)).ToList(),
                //    //}
                //})
                .Select(x => _mapper.Map<CartItemViewModel>(x))
                .ToListAsync();

            return Ok(cartItems);
        }

        // PUT: api/Cart/items/{itemId}
        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateCartItem(int itemId, CartItemViewModel itemViewModel)
        {
            var cart = await GetCurrentCartAsync();

            if (cart == null)
            {
                return NotFound("No se encontró un carrito activo para el usuario.");
            }

            // Verificar que el item pertenece al carrito del usuario
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.CartId == cart.Id);

            if (cartItem == null)
            {
                return NotFound("Item no encontrado en el carrito.");
            }

            // Actualizar cantidad
            cartItem.Quantity = itemViewModel.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            // Actualizar fecha del carrito
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/Cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddCartItem(CartItemViewModel itemViewModel)
        {
            var userId = GetCurrentUserId();
            int cartItemId;
            if (userId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            // Obtener carrito existente o crear uno nuevo
            var cart = await GetCurrentCartAsync();

            if (cart == null)
            {
                // Crear nuevo carrito
                cart = new Cart
                {
                    CustomerId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Verificar que el producto existe
            if (!itemViewModel.ProductId.HasValue)
            {
                return BadRequest("Se requiere ID de producto.");
            }

            var productExists = await _context.Products.AnyAsync(p => p.Id == itemViewModel.ProductId);
            if (!productExists)
            {
                return BadRequest("El producto no existe.");
            }

            // Verificar si ya existe el producto en el carrito
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(i => i.CartId == cart.Id &&
                                         i.ProductId == itemViewModel.ProductId &&
                                         i.VariantId == itemViewModel.VariantId);

            if (existingItem != null)
            {
                // Actualizar la cantidad si ya existe
                existingItem.Quantity = itemViewModel.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                cartItemId = existingItem.Id;
            }
            else
            {
                // Crear un nuevo item
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = itemViewModel.ProductId,
                    VariantId = itemViewModel.VariantId,
                    Quantity = itemViewModel.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CartItems.Add(cartItem);
                cartItemId = cartItem.Id;
            }

            // Actualizar la fecha de actualización del carrito
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Retornar el carrito actualizado
            var updatedCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var cartViewModel = _mapper.Map<CartViewModel>(updatedCart);
            var response = _context.CartItems.Where(x => x.CartId == cartItemId).Select(x => _mapper.Map<CartItemViewModel>(x));
            return Ok(response);
        }

        // DELETE: api/Cart/items/{itemId}
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveCartItem(int itemId)
        {
            var cart = await GetCurrentCartAsync();

            if (cart == null)
            {
                return NotFound("No se encontró un carrito activo para el usuario.");
            }

            // Verificar que el item pertenece al carrito del usuario
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.CartId == cart.Id);

            if (cartItem == null)
            {
                return NotFound("Item no encontrado en el carrito.");
            }

            _context.CartItems.Remove(cartItem);

            // Actualizar fecha del carrito
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}