using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class OrderDetailsService : IOrderDetailsService
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetOrderDetailsAsync()
        {
            var orderDetails = await _context.OrderDetails!
                .Include(od => od.order)
                .Include(od => od.product)
                .OrderBy(od => od.o_details_id)
                .ToListAsync();

            return orderDetails.Select(od => new
            {
                od.o_details_id,
                od.oid,
                od.pid,
                quantity = od.quantity,
                price = od.price,
                order = new
                {
                    od.order.oid,
                },
                product = new
                {
                    product_id = od.product.pid,
                }
            });
        }

        public async Task<OrderDetail?> GetOrderDetailByIdAsync(int id)
        {
            return await _context.OrderDetails!
                .Include(od => od.order)
                .Include(od => od.product)
                .FirstOrDefaultAsync(od => od.o_details_id == id);
        }

        public async Task<OrderDetail> AddOrderDetailAsync(OrderDetail orderDetail)
        {
            _context.OrderDetails!.Add(orderDetail);
            await _context.SaveChangesAsync();
            return orderDetail;
        }

        public async Task<bool> UpdateOrderDetailAsync(int id, OrderDetail orderDetail)
        {
            if (id != orderDetail.o_details_id)
            {
                return false;
            }

            _context.Entry(orderDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderDetailExists(id))
                {
                    return false;
                }

                throw;
            }
        }

        public async Task<bool> DeleteOrderDetailAsync(int id)
        {
            var orderDetail = await _context.OrderDetails!.FindAsync(id);
            if (orderDetail == null)
            {
                return false;
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();
            return true;
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails!.Any(e => e.o_details_id == id);
        }
    }
}
