using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Existing
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Spotify> Spotify { get; set; }

    // NEW catalog/cache sets
    public DbSet<TrackCatalog> TracksCatalog { get; set; } = null!;
    public DbSet<ArtistCatalog> ArtistsCatalog { get; set; } = null!;
    public DbSet<TrackArtist> TrackArtists { get; set; } = null!;
    public DbSet<ArtistGenre> ArtistGenres { get; set; } = null!;
    public DbSet<TrackGenreWeight> TrackGenreWeights { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- existing mappings (as you already had) ----
        modelBuilder.Entity<User>().HasKey(u => u.Uid);
        modelBuilder.Entity<User>().HasIndex(u => u.Uname).IsUnique();

        modelBuilder.Entity<Product>().HasKey(p => p.Pid);

        modelBuilder.Entity<Order>().HasKey(o => o.Oid);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(o => o.Orders)
            .HasForeignKey(o => o.Uid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetail>().HasKey(od => new { od.Oid, od.Pid });
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

        modelBuilder.Entity<Feedback>().HasKey(f => f.Fid);
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(f => f.Uid);

        modelBuilder.Entity<Spotify>().HasKey(s => s.Id);
        modelBuilder.Entity<Spotify>().HasIndex(s => s.TrackId);

        // ---- NEW: catalog/cache relationships ----
        modelBuilder.Entity<TrackCatalog>().HasKey(x => x.TrackId);
        modelBuilder.Entity<ArtistCatalog>().HasKey(x => x.ArtistId);

        modelBuilder.Entity<TrackArtist>().HasKey(x => new { x.TrackId, x.ArtistId });
        modelBuilder.Entity<TrackArtist>()
            .HasOne(x => x.Track)
            .WithMany()
            .HasForeignKey(x => x.TrackId);
        modelBuilder.Entity<TrackArtist>()
            .HasOne(x => x.Artist)
            .WithMany(a => a.TrackArtists)
            .HasForeignKey(x => x.ArtistId);

        modelBuilder.Entity<ArtistGenre>().HasKey(x => new { x.ArtistId, x.Genre });
        modelBuilder.Entity<ArtistGenre>()
            .HasOne(x => x.Artist)
            .WithMany(a => a.Genres)
            .HasForeignKey(x => x.ArtistId);
        modelBuilder.Entity<ArtistGenre>().HasIndex(x => x.Genre);

        modelBuilder.Entity<TrackGenreWeight>().HasKey(x => new { x.TrackId, x.Genre });
    }
}
