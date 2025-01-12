using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<User>? Users { get; set; }
        public DbSet<Customer>? Customers { get; set; }
        public DbSet<Product>? Products { get; set; }
        public DbSet<Order>? Orders { get; set; }
        public DbSet<OrderDetail>? OrderDetails { get; set; }

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
                entity.HasKey(e => e.c_id);
                entity.HasIndex(e => e.email).IsUnique();
            });

            // Products Table
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.p_id);
                entity.HasIndex(e => e.p_name).IsUnique();
                entity.ToTable(t =>
                    t.HasCheckConstraint("CHK_Product_Quantity_Price", "stock_quantity >= 0 AND price > 0"));
            });

            // Orders Table
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.o_id);

                entity.HasOne(o => o.customer)
                    .WithMany(c => c.orders)
                    .HasForeignKey(o => o.c_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderDetails Table
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.o_details_id);

                entity.HasOne(od => od.order)
                    .WithMany(o => o.order_details)
                    .HasForeignKey(od => od.o_id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.product)
                    .WithMany()
                    .HasForeignKey(od => od.p_id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(od => new { od.o_id, od.p_id })
                    .IsUnique();

                entity.ToTable(t =>
                    t.HasCheckConstraint("CHK_OrderDetail_Quantity_Price", "quantity > 0 AND price > 0"));
            });
        }
    }
}
