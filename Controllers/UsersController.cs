using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly JwtService _jwtService;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger, JwtService jwtService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }


        // API for user login using hashing
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            _logger.LogInformation($"Login attempt for username: {loginRequest.Username}");

            // Check if the user exists
            var user = await _context.Users!
                .FirstOrDefaultAsync(u => u.uname == loginRequest.Username);

            if (user == null)
            {
                _logger.LogWarning($"Invalid login attempt for non-existent user: {loginRequest.Username}");
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Check if the password matches
            if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.upass))
            {
                _logger.LogWarning($"Invalid password attempt for user: {loginRequest.Username}");
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Generate the JWT token
            var token = _jwtService.GenerateJwtToken(user.uname);

            _logger.LogInformation($"User {loginRequest.Username} logged in successfully.");
            return Ok(new { message = "Login successful!", token });
        }


        // Class for login data
        public class LoginRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users!.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }


        //Read (GET)
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users!.ToListAsync();
            return Ok(users);
        }


        // Create (POST) using hashing
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            user.upass = BCrypt.Net.BCrypt.HashPassword(user.upass);

            _context.Users!.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User created successfully!", user });
        }


        // Update (PUT) using hashing
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            // Find the user by ID
            var user = await _context.Users!.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // If the password is being updated, hash the new password
            if (!string.IsNullOrEmpty(updatedUser.upass))
            {
                updatedUser.upass = BCrypt.Net.BCrypt.HashPassword(updatedUser.upass);
            }
            // Update other user properties
            user.uname = updatedUser.uname;
            user.upass = updatedUser.upass;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully!", user });
        }


        //Delete (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users!.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deleted successfully!" });
        }

    }
}
