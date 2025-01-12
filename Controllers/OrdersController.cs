using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _context.Orders!
                .Include(o => o.customer)  // Include the Customer data
                .OrderBy(o => o.o_id)  // Order by o_id ascending
                .ToListAsync();

            // Create a simplified, flattened response without references
            var result = orders.Select(o => new
            {
                o.o_id,
                o.c_id,
                // Format the date correctly and assign it as a string
                o_date = o.o_date.ToString("yyyy-MM-dd HH:mm:ss"),  // Format the date here
                o.total_amount,
                // Convert enum to its string name
                customer = new
                {
                    o.customer.first_name,
                }
            }).ToList();

            return Ok(result);
        }

        // GET: api/Orders/:id
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders!
                                       .Include(o => o.customer)
                                       .Include(o => o.order_details)
                                       .ThenInclude(od => od.product)
                                       .FirstOrDefaultAsync(o => o.o_id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _context.Orders!.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.o_id }, order);
        }

        // PUT: api/Orders/:id
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.o_id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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

        // DELETE: api/Orders/:id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders!.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _context.Orders!.Any(e => e.o_id == id);
        }
    }
}
