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
        public DbSet<OrderDetail> OrderDetails { get; set; }

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

                entity.HasOne(c => c.user)
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

                entity.HasOne(o => o.customer)
                    .WithMany(c => c.orders)
                    .HasForeignKey(o => o.cid)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderDetails Table
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.o_details_id);

                entity.HasOne(od => od.order)
                    .WithMany(o => o.order_details)
                    .HasForeignKey(od => od.oid)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.product)
                    .WithMany()
                    .HasForeignKey(od => od.pid)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(od => new { od.oid, od.pid })
                    .IsUnique();

                entity.ToTable(t =>
                    t.HasCheckConstraint("CHK_OrderDetail_Quantity_Price", "quantity > 0 AND price > 0"));
            });
        }
    }
}
