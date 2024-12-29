using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.Extensions.Logging; // Ensure this is included for logging

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger; // Declare the logger

        // Inject the logger into the constructor
        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger; // Assign the logger to the private field
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            // Log the login attempt
            _logger.LogInformation($"Login attempt for username: {loginRequest.Username}");

            // Fetch the user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.uname == loginRequest.Username);

            // Check if user exists
            if (user == null)
            {
                _logger.LogWarning($"Invalid login attempt for non-existent a: {loginRequest.Username}");
                return Unauthorized("Invalid credentials");
            }

            // Check if the password matches
            if (user.upass != loginRequest.Password)
            {
                _logger.LogWarning($"Invalid password attempt for c: {loginRequest.Username}");
                return Unauthorized("Invalid credentials");
            }

            // Log successful login
            _logger.LogInformation($"User {loginRequest.Username} logged in successfully.");

            return Ok(new { message = "Login successful!" });
        }

        public class LoginRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }

    }
}









