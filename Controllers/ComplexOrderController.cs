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
                    // Creating the JSON
                    var productJson = JsonConvert.SerializeObject(request.Products);

                    // Logging the JSON for debugging
                    Console.WriteLine($"Products JSON: {productJson}");

                    // Defining parameters for the SQL query
                    var customerIdParam = new MySqlParameter("@CustomerId", MySqlDbType.Int32)
                    {
                        Value = request.CustomerId
                    };

                    var productsJsonParam = new MySqlParameter("@ProductsJson", MySqlDbType.JSON)
                    {
                        Value = productJson
                    };

                    // Executing the stored procedure
                    var result = await _context.Database.ExecuteSqlRawAsync(
                        "CALL CreateComplexOrder(@CustomerId, @ProductsJson)",
                        customerIdParam, productsJsonParam
                    );

                    // Committing the transaction
                    await transaction.CommitAsync();

                    return Ok(new { message = "Order placed successfully!" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Logging the error for debugging
                    Console.Error.WriteLine($"Error occurred: {ex.Message}");
                    Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");
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
