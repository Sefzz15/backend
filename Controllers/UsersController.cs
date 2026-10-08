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
        return Ok(users.Select(UserResponse.From));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        User? user = await userService.GetUserById(id);
        if (user == null) return NotFound();
        return Ok(UserResponse.From(user));
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
        return CreatedAtAction(nameof(GetUserById), new { id = user.Uid }, UserResponse.From(user));
    }


    // Passwords are deliberately NOT editable here - see PUT {id}/password.
    // The stored hash is carried over, so a client echoing back a stale value
    // cannot overwrite it (which used to re-hash the hash and lock the user out).
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
        if (id != user.Uid) return BadRequest();

        User? existing = await userService.GetUserById(id);
        if (existing == null) return NotFound();

        existing.Uname = user.Uname;

        await userService.UpdateUser(existing);
        return NoContent();
    }

    // Changing a password requires the account's current one, same rule as delete.
    [HttpPut("{id}/password")]
    public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordRequest? request)
    {
        if (string.IsNullOrEmpty(request?.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
            return BadRequest(new { message = "Current and new password are required." });
        }

        User? user = await userService.GetUserById(id);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Upass))
        {
            return Unauthorized(new { message = "Incorrect current password." });
        }

        user.Upass = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await userService.UpdateUser(user);
        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id, [FromBody] DeleteUserRequest? request)
    {
        if (string.IsNullOrEmpty(request?.Password))
        {
            return BadRequest(new { message = "Password is required." });
        }

        User? user = await userService.GetUserById(id);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Upass))
        {
            return Unauthorized(new { message = "Incorrect password for that account." });
        }

        await userService.DeleteUser(id);
        return NoContent();
    }
}