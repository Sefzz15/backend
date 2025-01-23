using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users Table
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.uid);
                entity.HasIndex(e => e.uname).IsUnique();
            });

            // Customers Table
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.cid);
                entity.HasIndex(e => e.email).IsUnique();

                entity.HasOne(c => c.User)
                    .WithOne()
                    .HasForeignKey<Customer>(c => c.uid)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Products Table
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.pid);
                entity.HasIndex(e => e.p_name).IsUnique();
                entity.ToTable(t =>
                    t.HasCheckConstraint("CHK_Product_Quantity_Price", "stock >= 0 AND price > 0"));
            });

            // Orders Table
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.oid);

                // Relationship Order -> Customer (Many-to-One)
                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.orders)
                    .HasForeignKey(o => o.cid)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of customer with orders

                // Relationship Order -> Product (Many-to-One)
                entity.HasOne(o => o.Product)
                    .WithMany()
                    .HasForeignKey(o => o.pid)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of product with orders
            });
        }
    }
}