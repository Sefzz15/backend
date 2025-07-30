using Microsoft.EntityFrameworkCore;

public class OrderDetailService
{
    private readonly AppDbContext _context;

    public OrderDetailService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDetail>> GetAllOrderDetails()
    {

        // return await _context.OrderDetails.ToListAsync();
        

        return await _context.OrderDetails
        .Include(od => od.Product)
        .Include(od => od.Order)
        .Include(od => od.Order.User)
        .ToListAsync();
    }
    public async Task<OrderDetail?> GetOrderDetailById(int id)
    {
        return await _context.OrderDetails.FindAsync(id);
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

        var orderDetails = new List<OrderDetail>();

        foreach (var detail in order.OrderDetails)
        {
            var product = await _context.Products.FindAsync(detail.Pid);
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
            var newOrderDetail = new OrderDetail
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