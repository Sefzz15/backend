using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy to allow requests from the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Άδεια στο Angular frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Retrieve connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Check if connection string is valid
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

// Add DbContext with MySQL provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString)
);

// Add controllers for API endpoints
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<JwtService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();




var app = builder.Build();

app.UseCors("AllowLocalhost");
app.UseRouting();
app.MapControllers();

app.MapControllers();  // Ρύθμιση των API endpoints

app.Run();
