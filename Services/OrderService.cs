using Microsoft.EntityFrameworkCore;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    // Get all orders
    public async Task<IEnumerable<Order>> GetAllOrders()
    {
        return await _context.Orders
                             .Include(o => o.User)
                             .ToListAsync();
    }

    // Get order by ID
    public async Task<Order?> GetOrderById(int id)
    {
        return await _context.Orders
                             .Include(o => o.User)
                             .FirstOrDefaultAsync(o => o.Oid == id);
    }

    // Add new order
    public async Task AddOrder(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    // Update existing order
    public async Task UpdateOrder(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    // Delete order by ID
    public async Task DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }

    // Get orders by user ID
    public async Task<IEnumerable<Order>> GetOrdersByUserId(int userId)
    {
        return await _context.Orders
                             .Include(o => o.User)
                             .Where(o => o.Uid == userId)
                             .ToListAsync();
    }
}
