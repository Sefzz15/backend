using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // API for user login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            _logger.LogInformation($"Login attempt for username: {loginRequest.Username}");

            // Check if user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.uname == loginRequest.Username);

            if (user == null)
            {
                _logger.LogWarning($"Invalid login attempt for non-existent user: {loginRequest.Username}");
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Check if password matches
            if (user.upass != loginRequest.Password)
            {
                _logger.LogWarning($"Invalid password attempt for user: {loginRequest.Username}");
                return Unauthorized(new { message = "Invalid credentials" });
            }

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
