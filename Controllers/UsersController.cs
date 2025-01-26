using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserById(id);
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

        var userId = await _userService.GetUserIdByUsernameAsync(username);

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

        var user = await _userService.LoginAsync(request.Username, request.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(new
        {
            message = "Login successful!",
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
        await _userService.AddUser(user);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Uid }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
        if (id != user.Uid) return BadRequest();
        await _userService.UpdateUser(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUser(id);
        return NoContent();
    }
}