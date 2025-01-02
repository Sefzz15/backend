using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        // DbSets representing the tables in the database
        public DbSet<User>? Users { get; set; }
        public DbSet<Customer>? Customers { get; set; }
        public DbSet<Product>? Products { get; set; }
        public DbSet<Order>? Orders { get; set; }
        public DbSet<OrderDetail>? OrderDetails { get; set; }

        // Fluent API Configuration (for additional restrictions, e.g., relationships, constraints)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Primary Key configuration
            modelBuilder.Entity<User>()
                .HasKey(u => u.uid);

            modelBuilder.Entity<Customer>()
                .HasKey(c => c.c_id);

            modelBuilder.Entity<Product>()
                .HasKey(p => p.p_id);

            modelBuilder.Entity<Order>()
                .HasKey(o => o.OId);

            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => od.ODetailsId);

            // Foreign Key relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.c_id)
                .OnDelete(DeleteBehavior.Cascade); // Foreign key relationship: Customer -> Order

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany()
                .HasForeignKey(od => od.OId)
                .OnDelete(DeleteBehavior.Cascade); // Foreign key relationship: Order -> OrderDetail

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.p_id); // Foreign key relationship: Product -> OrderDetail

            // Unique constraint on OrderDetail: Unique combination of o_id and p_id
            modelBuilder.Entity<OrderDetail>()
                .HasIndex(od => new { od.OId, od.p_id })
                .IsUnique();

            // Enum status field for Orders
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>(); // Store status as string in DB (Pending, Processing, etc.)
        }
    }
}
