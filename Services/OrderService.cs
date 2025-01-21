using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetOrdersAsync()
        {
            var orders = await _context.Orders!
                .Include(o => o.customer)
                .OrderBy(o => o.oid)
                .ToListAsync();

            return orders.Select(o => new
            {
                o.oid,
                o.cid,
                o_date = o.o_date.ToString("yyyy-MM-dd HH:mm:ss"),
                o.total_amount,
                customer = new
                {
                    o.customer.first_name,
                }
            });
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders!
                .Include(o => o.customer)
                .Include(o => o.order_details)
                .ThenInclude(od => od.product)
                .FirstOrDefaultAsync(o => o.oid == id);
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            _context.Orders!.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> UpdateOrderAsync(int id, Order order)
        {
            if (id != order.oid)
            {
                return false;
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return false;
                }

                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders!.FindAsync(id);
            if (order == null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        private bool OrderExists(int id)
        {
            return _context.Orders!.Any(e => e.oid == id);
        }
    }
}
