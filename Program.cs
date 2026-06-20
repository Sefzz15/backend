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


// Optional: import Spotify data.
//   Windows (host):  dotnet run -- --import-spotify              → opens a folder picker
//   Explicit folder: dotnet run -- --import-spotify "<folder>"   → imports from that folder
//   Docker/Linux:    pass the folder (mounted into the container) or set SPOTIFY_IMPORT_DIR,
//                    because the GUI folder picker can't run in a headless container.
if (args.Contains("--import-spotify"))
{
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    string? dir = ImportDirFromArgs(args) ?? Environment.GetEnvironmentVariable("SPOTIFY_IMPORT_DIR");
    if (!string.IsNullOrWhiteSpace(dir))
        await SpotifyImporter.ImportAsync(db, dir);
    else
        await SpotifyImporter.ImportAsync(db);   // no folder given → Windows folder picker
    return;

    // Accepts either "--import-spotify <folder>" or "--import-spotify=<folder>".
    static string? ImportDirFromArgs(string[] args)
    {
        const string flag = "--import-spotify";
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith(flag + "=", StringComparison.Ordinal))
                return args[i][(flag.Length + 1)..];
            if (args[i] == flag && i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                return args[i + 1];
        }
        return null;
    }
}

// Pipeline
app.UseCors("AllowLocalhost");
app.UseRouting();
app.MapControllers();

app.Run();