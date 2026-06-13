using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(UserService userService, JwtService jwtService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        IEnumerable<User> users = await userService.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        User? user = await userService.GetUserById(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    // New endpoint for getting user ID by username
    [HttpGet("getIdByUsername/{username}")]
    public async Task<IActionResult> GetUserIdByUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Username is required." });
        }

        int? userId = await userService.GetUserIdByUsernameAsync(username);

        if (userId == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new { userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Username and Password are required." });
        }

        User? user = await userService.GetUserByUsernameAsync(request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Upass))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        string token = jwtService.GenerateJwtToken(user.Uname);

        return Ok(new
        {
            message = "Login successful!",
            token,
            user = new
            {
                user.Uid,
                user.Uname
            }
        });
    }


    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        if (string.IsNullOrEmpty(user.Uname) || string.IsNullOrEmpty(user.Upass))
        {
            return BadRequest(new { message = "Username and Password are required." });
        }

        if (await userService.UsernameExistsAsync(user.Uname))
        {
            return Conflict(new { message = "Username already exists." });
        }

        // Hash the password before saving
        user.Upass = BCrypt.Net.BCrypt.HashPassword(user.Upass);

        await userService.AddUser(user);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Uid }, user);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
        if (id != user.Uid) return BadRequest();

        if (!string.IsNullOrEmpty(user.Upass))
        {
            user.Upass = BCrypt.Net.BCrypt.HashPassword(user.Upass);
        }

        await userService.UpdateUser(user);
        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await userService.DeleteUser(id);
        return NoContent();
    }
}