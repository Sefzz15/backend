using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Data;
using MySql.Data.MySqlClient;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not defined in appsettings.json.");
}

// Register DbContext with MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));

// Register JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key")),
        };
    });

// Register MVC Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Use CORS to allow frontend from Angular
app.UseCors(builder =>
    builder.WithOrigins("http://localhost:4200") // Angular URL
           .AllowAnyHeader()
           .AllowAnyMethod());

// Use Authentication Middleware
app.UseAuthentication();

// Use Authorization Middleware
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Start the app
app.Run();
