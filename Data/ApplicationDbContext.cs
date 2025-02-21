using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithMany(u => u.Customers)
            .HasForeignKey(c => c.Uid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.Cid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
               .HasOne(oi => oi.Order) // Each OrderItem has one Order
               .WithMany(o => o.OrderItems) // Each Order has many OrderItems
               .HasForeignKey(oi => oi.OrderId) // ForeignKey in OrderItem
               .OnDelete(DeleteBehavior.Cascade); // Optional: Define delete behavior

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product) // Each OrderItem has one Product
            .WithMany() // No need for reverse navigation in Product
            .HasForeignKey(oi => oi.ProductId); // ForeignKey in OrderItem
    }
}