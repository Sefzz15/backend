using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomersController> _logger;
        private readonly JwtService _jwtService;

        public CustomersController(ApplicationDbContext context, ILogger<CustomersController> logger, JwtService jwtService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers!.FindAsync(id);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }
            return Ok(customer);
        }


        // Read (GET)
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _context.Customers!.ToListAsync();
            return Ok(customers);
        }


        // Create (POST)
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (customer == null)
            {
                _logger.LogError("Customer data is null.");
                return BadRequest("Invalid customer data.");
            }

            _logger.LogInformation("Received customer data: {firstName} {lastName}, email: {email}, uid: {uid}",
                customer.first_name, customer.last_name, customer.email, customer.uid);

            try
            {
                _context.Customers!.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer successfully created with ID: {customerId}", customer.cid);
                return Ok(new { message = "Customer created successfully!", customer });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while saving customer: {errorMessage}", ex.Message);
                return StatusCode(500, "An error occurred while creating the customer.");
            }
        }



        // Update (PUT) using hashing
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer updatedCustomer)
        {
            // Find the customer by ID
            var customer = await _context.Customers!.FindAsync(id);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            // Update other customer properties
            customer.cid = updatedCustomer.cid;
            customer.first_name = updatedCustomer.first_name;
            customer.last_name = updatedCustomer.last_name;
            customer.email = updatedCustomer.email;
            customer.phone = updatedCustomer.phone;
            customer.address = updatedCustomer.address;
            customer.city = updatedCustomer.city;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Customer updated successfully!", customer });
        }


        //Delete (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers!.FindAsync(id);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            _context.Customers!.Remove(customer);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Customer deleted successfully!" });
        }

    }
}
