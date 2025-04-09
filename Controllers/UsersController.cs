using backend.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly CustomerService _customerService;
    private readonly JwtService _jwtService;

    public UserController(UserService userService, CustomerService customerService, JwtService jwtService)
    {
        _userService = userService;
        _customerService = customerService;
        _jwtService = jwtService;

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


    [HttpGet("check-customer/{userId}")]
    public async Task<ActionResult<Customer>> CheckIfUserIsCustomer(int userId)
    {
        var customer = await _customerService.GetCustomerByUserIdAsync(userId);

        if (customer == null)
        {
            return NotFound("User is not a customer.");
        }

        return Ok(customer);
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
        var token = _jwtService.GenerateJwtToken(user.Uname);

        return Ok(new
        {
            message = "Login successful!",
            token = token,
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