public class OrderItemService
{
    private readonly AppDbContext _context;

    public OrderItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderItem>> CreateOrderItemsAsync(Order order)
    {
        var orderItems = new List<OrderItem>();

        foreach (var orderItem in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(orderItem.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {orderItem.ProductId} not found.");
            }

            if (product.Stock < orderItem.Quantity)
            {
                throw new ArgumentException($"Not enough stock for product {product.Pname}.");
            }

            // Deduct stock
            product.Stock -= orderItem.Quantity;

            // Create and populate the OrderItem with the correct OrderId
            var newOrderItem = new OrderItem
            {
                ProductId = orderItem.ProductId,
                Quantity = orderItem.Quantity,
                OrderId = order.Oid  // Make sure to set the OrderId from the Order's Oid
            };

            orderItems.Add(newOrderItem);
        }

        // Update the product stock in the database
        await _context.SaveChangesAsync();

        return orderItems;
    }
}