using Microsoft.EntityFrameworkCore;
using backend.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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
builder.Services.AddScoped<SpotifyService>();

builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection("Spotify"));


// HttpClient(s)
builder.Services.AddHttpClient<SpotifyCatalogService>(client =>
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
var app = builder.Build();


// Optional: import Spotify data
if (args.Contains("--import-spotify"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SpotifyImporter.ImportAsync(db);
    return;
}

// Pipeline
app.UseCors("AllowLocalhost");
app.UseRouting();
app.MapControllers();

app.Run();