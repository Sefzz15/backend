using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplexOrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ComplexOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComplexOrder([FromBody] ComplexOrderRequest request)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Create JSON string for products
                    var productJson = JsonConvert.SerializeObject(request.Products);

                    // Log for debugging
                    Console.WriteLine($"Products JSON: {productJson}");

                    // Prepare parameters for the SQL call
                    var customerIdParam = new MySqlParameter("@CustomerId", MySqlDbType.Int32)
                    {
                        Value = request.CustomerId
                    };

                    var productsJsonParam = new MySqlParameter("@ProductsJson", MySqlDbType.JSON)
                    {
                        Value = productJson
                    };

                    // Execute the stored procedure
                    var result = await _context.Database.ExecuteSqlRawAsync(
                        "CALL CreateComplexOrder(@CustomerId, @ProductsJson)",
                        customerIdParam, productsJsonParam
                    );

                    // If the procedure succeeds, commit the transaction
                    await transaction.CommitAsync();

                    // Return success message to client
                    return Ok(new { message = "Order placed successfully!" });
                }
                catch (MySqlException ex)
                {
                    // Logging for the MySQL error
                    Console.Error.WriteLine($"MySQL Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { message = "Database error while placing the order.", error = ex.Message });
                }

                catch (Exception ex)
                {
                    // Handle unexpected errors
                    await transaction.RollbackAsync();
                    Console.Error.WriteLine($"Error: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to place order.", error = ex.Message });
                }
            }
        }

        // Models for the request payload
        public class ComplexOrderRequest
        {
            public int CustomerId { get; set; }
            public List<ProductOrderRequest> Products { get; set; } = new();
        }

        public class ProductOrderRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
