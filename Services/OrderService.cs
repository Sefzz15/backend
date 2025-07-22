using Microsoft.EntityFrameworkCore;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    // Λήψη όλων των παραγγελιών (μαζί με τον χρήστη)
    public async Task<IEnumerable<Order>> GetAllOrders()
    {
        return await _context.Orders
                             .Include(o => o.User)
                             .ToListAsync();
    }

    // Λήψη παραγγελίας με βάση το ID
    public async Task<Order?> GetOrderById(int id)
    {
        return await _context.Orders
                             .Include(o => o.User)
                             .FirstOrDefaultAsync(o => o.Oid == id);
    }

    // Προσθήκη παραγγελίας
    public async Task AddOrder(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    // Ενημέρωση παραγγελίας
    public async Task UpdateOrder(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    // Διαγραφή παραγγελίας
    public async Task DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }

    // (Προαιρετικά) Λήψη παραγγελιών ενός συγκεκριμένου χρήστη
    public async Task<IEnumerable<Order>> GetOrdersByUserId(int userId)
    {
        return await _context.Orders
                             .Include(o => o.User)
                             .Where(o => o.Uid == userId)
                             .ToListAsync();
    }
}
