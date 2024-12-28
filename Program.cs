using Microsoft.EntityFrameworkCore;
using backend.Data;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not defined in appsettings.json.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(builder =>
    builder.WithOrigins("http://localhost:4200") // Angular URL
           .AllowAnyHeader()
           .AllowAnyMethod());

app.UseAuthorization();

app.MapControllers();

app.Run();

// try
// {
//     using (MySqlConnection conn = new MySqlConnection(connectionString))
//     {
//         conn.Open();
//         Console.WriteLine("Η σύνδεση με τη βάση δεδομένων είναι επιτυχής.");
//     }
// }
// catch (Exception ex)
// {
//     Console.WriteLine($"Σφάλμα κατά τη σύνδεση: {ex.Message}");
// }