using Microsoft.EntityFrameworkCore;

public class CustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        return await _context.Customers.Include(c => c.User).ToListAsync();
    }

    public async Task<Customer?> GetCustomerById(int id)
    {
        return await _context.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.Cid == id);
    }

    public async Task AddCustomer(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCustomer(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }

        public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
    {
        return await _context.Customers
            .Where(c => c.Uid == userId)
            .Select(c => new Customer
            {
            Cid = c.Cid,
            Uid = c.Uid,
            Firstname = c.Firstname,
            Lastname = c.Lastname,
            Email = c.Email
            })
            .FirstOrDefaultAsync();
    }
}