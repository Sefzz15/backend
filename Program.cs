using Microsoft.EntityFrameworkCore;
using backend.Services;
using System.Text.Json.Serialization;
using backend.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// CORS — allowed origins come from configuration (Cors:AllowedOrigins, comma-separated); defaults to local dev.
string[] allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Logging
builder.Logging.AddSimpleConsole(o =>
{
    o.TimestampFormat = "[HH:mm:ss] ";
    o.SingleLine = true;
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


// Configuration / Connection string
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString)
);

// MVC / JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// DI registrations
builder.Services.AddLogging();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<JwtService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderDetailService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<SpotifyQueryService>();

builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection("Spotify"));


// HttpClient(s)
builder.Services.AddHttpClient<SpotifyEnricher>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(100);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    MaxConnectionsPerServer = 10
});
// .ConfigurePrimaryHttpMessageHandler is not tested

// Build app
WebApplication app = builder.Build();

// Apply pending EF Core migrations on startup so a fresh MySQL container gets its schema.
// Retry briefly to tolerate the database container still warming up.
using (IServiceScope migrationScope = app.Services.CreateScope())
{
    AppDbContext db = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
    for (int attempt = 1; ; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < 10)
        {
            app.Logger.LogWarning(ex, "Database not ready (attempt {Attempt}/10), retrying in 3s...", attempt);
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }
}


// Optional: import Spotify data
if (args.Contains("--import-spotify"))
{
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SpotifyImporter.ImportAsync(db);
    return;
}

// Pipeline
app.UseCors("AllowLocalhost");
app.UseRouting();
app.MapControllers();

app.Run();