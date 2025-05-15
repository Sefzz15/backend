
using Microsoft.EntityFrameworkCore;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Get all orders with customer and order items
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)  // Include customer details
            .Include(o => o.OrderItems) // Include order items
                .ThenInclude(oi => oi.Product) // Include product details in order items
            .ToListAsync();
    }

    public async Task<Order?> GetOrderById(int id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) 
            .FirstOrDefaultAsync(o => o.Oid == id);
    }

    public async Task AddOrder(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrder(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}