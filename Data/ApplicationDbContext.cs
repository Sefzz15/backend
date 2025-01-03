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
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.uid); // Set primary key explicitly
            });
            // Customers Table
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.c_id); // c_id
                entity.Property(e => e.first_name).HasMaxLength(50);
                entity.Property(e => e.last_name).HasMaxLength(50);
                entity.Property(e => e.email).HasMaxLength(100);
                entity.Property(e => e.phone).HasMaxLength(15);
                entity.Property(e => e.address).HasMaxLength(255);
                entity.Property(e => e.city).HasMaxLength(50);
            });

            // Products Table
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.p_id); // p_id
                entity.Property(e => e.p_name).HasMaxLength(100);
                entity.Property(e => e.description).HasColumnType("TEXT");
                entity.Property(e => e.price).HasColumnType("DECIMAL(10,2)");
                entity.Property(e => e.stock_quantity).IsRequired();
            });

            // Orders Table
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.o_id); // o_id
                entity.Property(e => e.o_date).IsRequired(); // o_date
                entity.Property(e => e.total_amount).HasColumnType("DECIMAL(10,2)");
                entity.Property(e => e.status)
                      .HasMaxLength(20)
                      .HasDefaultValue("Pending"); // status

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(e => e.c_id) // c_id
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderDetails Table
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.o_details_id); // o_details_id

                entity.Property(e => e.quantity).IsRequired(); // quantity
                entity.Property(e => e.price).HasColumnType("DECIMAL(10,2)"); // price

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.o_id) // o_id
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.Product)
                      .WithMany()
                      .HasForeignKey(od => od.p_id) // p_id
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(od => new { od.o_id, od.p_id }).IsUnique();
            });
        }

    }
}
