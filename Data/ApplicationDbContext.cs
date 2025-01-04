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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users Table
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.uid);
                entity.HasIndex(e => e.uname)
                       .IsUnique();
            });

            // Customers Table
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.c_id);

                entity.Property(e => e.first_name)
                      .HasMaxLength(50);

                entity.Property(e => e.last_name)
                      .HasMaxLength(50);

                entity.Property(e => e.email)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(e => e.email)
                      .IsUnique();

                entity.Property(e => e.phone)
                      .HasMaxLength(15)
                      .IsRequired(false);

                entity.Property(e => e.address)
                      .HasMaxLength(255)
                      .IsRequired(false);

                entity.Property(e => e.city)
                      .HasMaxLength(50);
            });

            // Products Table
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.p_id);

                entity.Property(e => e.p_name)
                      .HasMaxLength(50);

                entity.HasIndex(e => e.p_name)
                      .IsUnique();

                entity.Property(e => e.description)
                      .HasColumnType("TEXT");

                entity.Property(e => e.price)
                      .HasColumnType("DECIMAL(10,2)");

                entity.Property(e => e.stock_quantity)
                      .IsRequired();

                entity.ToTable(
                    "Products",
                    t => t.HasCheckConstraint("CHK_Product_Quantity_Price", "stock_quantity >= 0 AND price > 0")
                );
            });

            // Orders Table
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.o_id);

                entity.Property(e => e.o_date)
                      .HasColumnType("TIMESTAMP")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .IsRequired();

                entity.Property(e => e.total_amount)
                      .HasColumnType("DECIMAL(10,2)");

                entity.Property(e => e.status)
                      .HasConversion(
                          v => v.ToString(),
                          v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v))
                      .HasMaxLength(20)
                      .HasDefaultValue(OrderStatus.Pending);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(e => e.c_id)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderDetails Table
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.o_details_id);

                entity.Property(e => e.quantity)
                      .IsRequired();

                entity.Property(e => e.price)
                      .HasColumnType("DECIMAL(10,2)");

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.o_id)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.Product)
                      .WithMany()
                      .HasForeignKey(od => od.p_id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(od => new { od.o_id, od.p_id })
                      .IsUnique();

                entity.ToTable(t => t.HasCheckConstraint("CHK_OrderDetail_Quantity_Price", "quantity > 0 AND price > 0"));
            });

        }
    }
}
