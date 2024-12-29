using backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy to allow requests from the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
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

var app = builder.Build();

// Enable CORS
app.UseCors("AllowLocalhost");

// Enable routing and map controllers
app.UseRouting();
app.MapControllers();

app.Run();
