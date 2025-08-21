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
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Spotify> Spotify { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User primary key
        modelBuilder.Entity<User>()
            .HasKey(u => u.Uid);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Uname)
            .IsUnique();

        // Configure Product primary key
        modelBuilder.Entity<Product>()
            .HasKey(p => p.Pid);

        // Configure Order primary key and relationships
        modelBuilder.Entity<Order>()
            .HasKey(o => o.Oid);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(o => o.Orders)
            .HasForeignKey(o => o.Uid)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure OrderDetail
        modelBuilder.Entity<OrderDetail>()
            .HasKey(od => new { od.Oid, od.Pid });

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(od => od.OrderDetails)
            .HasForeignKey(od => od.Oid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(od => od.OrderDetails)
            .HasForeignKey(od => od.Pid)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Feedback
        modelBuilder.Entity<Feedback>()
       .HasKey(f => f.Fid);

        // Configure Spotify primary key
        modelBuilder.Entity<Spotify>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(f => f.Uid);
    }
}
