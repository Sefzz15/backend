using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;
        private readonly JwtService _jwtService;

        public UsersController(IUserService userService, ILogger<UsersController> logger, JwtService jwtService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        // API for user login using hashing
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            _logger.LogInformation($"Login attempt for username: {loginRequest.Username}");

            var user = await _userService.GetUserByUsernameAsync(loginRequest.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.upass))
            {
                _logger.LogWarning($"Invalid login attempt for user: {loginRequest.Username}");
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Generate JWT Token
            var token = _jwtService.GenerateJwtToken(user.uname);

            _logger.LogInformation($"User {loginRequest.Username} (ID: {user.uid}) logged in successfully.");
            return Ok(new { message = "Login successful!", token, userID = user.uid });
        }

        // Read (GET) all users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // Read (GET) user by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        // Create (POST) a new user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
                return BadRequest("Invalid user data.");

            var createdUser = await _userService.CreateUserAsync(user);
            return Ok(new { message = "User created successfully!", createdUser });
        }

        // Update (PUT) a user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            var user = await _userService.UpdateUserAsync(id, updatedUser);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new { message = "User updated successfully!", user });
        }

        // Delete (DELETE) a user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound("User not found.");

            return Ok(new { message = "User deleted successfully!" });
        }

        // Login Request DTO
        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
