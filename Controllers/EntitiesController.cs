using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EntitiesController> _logger;
        private readonly JwtService _jwtService;

        public EntitiesController(ApplicationDbContext context, ILogger<EntitiesController> logger, JwtService jwtService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }


        // API for login without hashing
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            _logger.LogInformation($"Login attempt for username: {loginRequest.Username}");

            // Check if the user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.uname == loginRequest.Username);

            if (user == null)
            {
                _logger.LogWarning($"Invalid login attempt for non-existent user: {loginRequest.Username}");
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Check if the password matches
            if (user.upass != loginRequest.Password)
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
            var user = await _context.Users.FindAsync(id);
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
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }


        //Create (POST) without hashing
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User created successfully!", user });
        }


        //Update (PUT) without hashing
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.uname = updatedUser.uname;
            user.upass = updatedUser.upass;

            await _context.SaveChangesAsync();
            return Ok(new { message = "User updated successfully!", user });
        }


        //Delete (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
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
