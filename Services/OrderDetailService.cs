using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderDetailService(AppDbContext context)
{
    public async Task<IEnumerable<OrderDetail>> GetAllOrderDetails()
    {
        return await context.OrderDetails
            .Include(od => od.Product)
            .Include(od => od.Order)
                .ThenInclude(o => o!.User)
            .ToListAsync();
    }

    public async Task<OrderDetail?> GetOrderDetailById(int id)
    {
        return await context.OrderDetails.FindAsync(id);
    }

    public async Task<List<OrderDetail>> CreateOrderDetailsAsync(Order order)
    {
        if (order == null)
        {
            throw new ArgumentException("Order is null.");
        }

        if (order.OrderDetails == null || !order.OrderDetails.Any())
        {
            throw new ArgumentException("Order should contain at least one product.");
        }

        List<OrderDetail> orderDetails = new List<OrderDetail>();

        foreach (OrderDetail detail in order.OrderDetails)
        {
            Product? product = await context.Products.FindAsync(detail.Pid);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {detail.Pid} not found.");
            }

            if (product.Stock < detail.Quantity)
            {
                throw new ArgumentException($"Stock is not sufficient for product {product.Pname}.");
            }

            // Lower the stock of the product
            product.Stock -= detail.Quantity;

            // Create the OrderDetail object
            OrderDetail newOrderDetail = new OrderDetail
            {
                Pid = detail.Pid,
                Quantity = detail.Quantity,
                Oid = order.Oid // Assuming Oid is set in the Order object
            };

            orderDetails.Add(newOrderDetail);
        }

        return orderDetails;
    }
}