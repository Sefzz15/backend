// using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Mvc;
// using MySql.Data.MySqlClient;
// using backend.Data;
// using backend.Models;
// using backend.Services;

// namespace backend.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class CustomerController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly ILogger<CustomerController> _logger;
//         private readonly JwtService _jwtService;

//         public CustomerController(ApplicationDbContext context, ILogger<CustomerController> logger, JwtService jwtService)
//         {
//             _context = context ?? throw new ArgumentNullException(nameof(context));
//             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//             _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
//         }

//         [HttpPost("create-user-and-customer")]
//         public async Task<IActionResult> CreateUserAndCustomer([FromBody] Customer customer)
//         {
//             if (customer == null)
//             {
//    
//             _logger.LogWarning("Received null customer object");
//                 return BadRequest("Invalid customer data.");
//             }

//             _logger.LogInformation("Received customer data: {FirstName}, {LastName}, {Email}, {Phone}, {Address}, {City}",
//                 customer.first_name, customer.last_name, customer.email, customer.phone, customer.address, customer.city);

//             if (string.IsNullOrWhiteSpace(customer.first_name))
//             {
//                 _logger.LogWarning("First name is empty or null.");
//                 return BadRequest("First name is required.");
//             }

//             try
//             {
//                 var firstnameParam = new MySqlParameter("@firstname", MySqlDbType.VarChar, 50) { Value = customer.first_name };
//                 var lastnameParam = new MySqlParameter("@lastname", MySqlDbType.VarChar, 50) { Value = customer.last_name };
//                 var emailParam = new MySqlParameter("@email", MySqlDbType.VarChar, 50) { Value = customer.email };
//                 var phoneParam = new MySqlParameter("@phone", MySqlDbType.VarChar, 15) { Value = customer.phone ?? (object)DBNull.Value };
//                 var addressParam = new MySqlParameter("@address", MySqlDbType.VarChar, 255) { Value = customer.address ?? (object)DBNull.Value };
//                 var cityParam = new MySqlParameter("@city", MySqlDbType.VarChar, 50) { Value = customer.city };

//                 _logger.LogInformation("Calling stored procedure CreateUserAndCustomer.");

//                 await _context.Database.ExecuteSqlRawAsync(
//                     "CALL CreateUserAndCustomer(@firstname, @lastname, @email, @phone, @address, @city)",
//                     firstnameParam, lastnameParam, emailParam, phoneParam, addressParam, cityParam
//                 );

//                 _logger.LogInformation("Stored procedure executed successfully.");
//                 return Ok(new { message = "User and customer created successfully!" });
//             }
//             catch (MySqlException ex)
//             {
//                 _logger.LogError("Database error during CreateUserAndCustomer: {Message}", ex.Message);
//                 return StatusCode(500, new { message = "Error creating user and customer.", error = ex.Message });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError("Unexpected error during CreateUserAndCustomer: {Message}", ex.Message);
//                 return StatusCode(500, new { message = "Failed to create user and customer.", error = ex.Message });
//             }
//         }









//         public class CustomerDto
//         {
//             public string? FirstName { get; set; }
//             public string? LastName { get; set; }
//             public string? Email { get; set; }
//             public string? Phone { get; set; }
//             public string? Address { get; set; }
//             public string? City { get; set; }
//         }
//     }
// }
