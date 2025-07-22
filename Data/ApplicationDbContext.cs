using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSets with matching property names
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User primary key
        modelBuilder.Entity<User>()
            .HasKey(u => u.Uid);

        // Configure Product primary key
        modelBuilder.Entity<Product>()
            .HasKey(p => p.Pid);

        // Configure Order primary key and relationships
        modelBuilder.Entity<Order>()
            .HasKey(o => o.Oid);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.Uid)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure OrderDetail
        modelBuilder.Entity<OrderDetail>()
            .HasKey(od => new { od.Oid, od.ProductId });

        modelBuilder.Entity<OrderDetail>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(oi => oi.Oid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // To avoid product deletion if referenced
    }
}
