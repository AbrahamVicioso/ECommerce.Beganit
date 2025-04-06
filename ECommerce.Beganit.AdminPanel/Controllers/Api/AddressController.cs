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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AddressController : ControllerBase
    {
        private readonly ECommerceDBContext _context;
        private readonly IMapper _mapper;

        public AddressController(ECommerceDBContext eCommerceDBContext, IMapper mapper)
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

        // GET: api/Address
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerAddressViewModel>>> GetAddresses()
        {
            var customerId = GetCurrentUserId();
            if (customerId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            var addresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            var addressViewModels = _mapper.Map<List<CustomerAddressViewModel>>(addresses);
            return Ok(addressViewModels);
        }

        // GET: api/Address/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerAddressViewModel>> GetAddress(int id)
        {
            var customerId = GetCurrentUserId();
            if (customerId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);

            if (address == null)
            {
                return NotFound("Dirección no encontrada.");
            }

            var addressViewModel = _mapper.Map<CustomerAddressViewModel>(address);
            return Ok(addressViewModel);
        }

        // POST: api/Address
        [HttpPost]
        public async Task<ActionResult<CustomerAddressViewModel>> CreateAddress(CustomerAddressViewModel addressViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerId = GetCurrentUserId();
            if (customerId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            // Asegurar que la dirección pertenezca al cliente autenticado
            addressViewModel.CustomerId = customerId;

            // Si esta dirección se marca como predeterminada, desmarcar las otras
            if (addressViewModel.IsDefault == true)
            {
                var defaultAddresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId && a.IsDefault == true)
                    .ToListAsync();

                foreach (var addr in defaultAddresses)
                {
                    addr.IsDefault = false;
                }
            }

            // Crear la nueva dirección
            var customerAddress = _mapper.Map<CustomerAddress>(addressViewModel);
            _context.CustomerAddresses.Add(customerAddress);
            await _context.SaveChangesAsync();

            // Mapear la entidad creada de vuelta al ViewModel para devolverla
            var createdAddressViewModel = _mapper.Map<CustomerAddressViewModel>(customerAddress);

            return CreatedAtAction(nameof(GetAddress), new { id = customerAddress.Id }, createdAddressViewModel);
        }

        // PUT: api/Address/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, CustomerAddressViewModel addressViewModel)
        {
            if (id != addressViewModel.Id)
            {
                return BadRequest("El ID de la dirección no coincide con el ID en la URL.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerId = GetCurrentUserId();
            if (customerId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            // Verificar que la dirección existe y pertenece al cliente
            var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);

            if (address == null)
            {
                return NotFound("Dirección no encontrada o no pertenece al usuario.");
            }

            // Si esta dirección se marca como predeterminada, desmarcar las otras
            if (addressViewModel.IsDefault == true && address.IsDefault != true)
            {
                var defaultAddresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId && a.IsDefault == true && a.Id != id)
                    .ToListAsync();

                foreach (var addr in defaultAddresses)
                {
                    addr.IsDefault = false;
                }
            }

            // Mantener el CustomerId original y actualizar los demás campos
            addressViewModel.CustomerId = customerId;
            _mapper.Map(addressViewModel, address);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerAddressExists(id))
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

        // DELETE: api/Address/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var customerId = GetCurrentUserId();
            if (customerId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);

            if (address == null)
            {
                return NotFound("Dirección no encontrada o no pertenece al usuario.");
            }

            _context.CustomerAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Address/default
        [HttpGet("default")]
        public async Task<ActionResult<CustomerAddressViewModel>> GetDefaultAddress()
        {
            var customerId = GetCurrentUserId();
            if (customerId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            var defaultAddress = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault == true);

            if (defaultAddress == null)
            {
                // Si no hay dirección predeterminada, devolver la primera dirección
                defaultAddress = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.CustomerId == customerId);

                if (defaultAddress == null)
                {
                    return NotFound("No se encontraron direcciones para el usuario.");
                }
            }

            var addressViewModel = _mapper.Map<CustomerAddressViewModel>(defaultAddress);
            return Ok(addressViewModel);
        }

        // PUT: api/Address/{id}/setdefault
        [HttpPut("{id}/setdefault")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var customerId = GetCurrentUserId();
            if (customerId == 0)
            {
                return Forbid("Usuario no autenticado.");
            }

            // Verificar que la dirección existe y pertenece al cliente
            var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);

            if (address == null)
            {
                return NotFound("Dirección no encontrada o no pertenece al usuario.");
            }

            // Desmarcar todas las direcciones como predeterminadas
            var allAddresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            foreach (var addr in allAddresses)
            {
                addr.IsDefault = addr.Id == id;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool CustomerAddressExists(int id)
        {
            return _context.CustomerAddresses.Any(e => e.Id == id);
        }
    }
}