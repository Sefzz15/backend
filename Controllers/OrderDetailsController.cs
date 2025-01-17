using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/OrderDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetails()
        {
            var orderDetails = await _context.OrderDetails!
                .Include(od => od.order)    // Include the Order data
                .Include(od => od.product)  // Include the Product data
                .OrderBy(od => od.o_details_id)
                .ToListAsync();

            // Simplified response
            var result = orderDetails.Select(od => new
            {
                od.o_details_id,
                od.o_id,
                od.p_id,
                quantity = od.quantity,
                price = od.price,
                order = new
                {
                    od.order.o_id, // Accessing the navigation property correctly
                },
                product = new
                {
                    product_id = od.product.p_id,
                }
            }).ToList();

            return Ok(result);
        }

        // GET: api/OrderDetails/:id
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(int id)
        {
            var orderDetail = await _context.OrderDetails!
                .Include(od => od.order)
                .Include(od => od.product)
                .FirstOrDefaultAsync(od => od.o_details_id == id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            return Ok(orderDetail); // Ensure response type matches the ActionResult
        }

        // POST: api/OrderDetails
        [HttpPost]
        public async Task<ActionResult<OrderDetail>> PostOrderDetail(OrderDetail orderDetail)
        {
            _context.OrderDetails!.Add(orderDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.o_details_id }, orderDetail);
        }

        // PUT: api/OrderDetails/:id
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderDetail(int id, OrderDetail orderDetail)
        {
            if (id != orderDetail.o_details_id)
            {
                return BadRequest();
            }

            _context.Entry(orderDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderDetailExists(id))
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

        // DELETE: api/OrderDetails/:id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var orderDetail = await _context.OrderDetails!.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails!.Any(e => e.o_details_id == id);
        }
    }
}
