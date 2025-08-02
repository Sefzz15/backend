using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly FeedbackService _feedbackService;

    private readonly AppDbContext _context;

    public FeedbackController(AppDbContext context, FeedbackService feedbackService)
    {
        _context = context;
        _feedbackService = feedbackService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllFeedbacks()
    {
        var feedbacks = await _feedbackService.GetAllFeedbacks();
        return Ok(feedbacks);
    }
    [HttpPost]
    public async Task<IActionResult> PostFeedback([FromBody] Feedback feedback)
    {
        // Optional: ensure the user exists
        var userExists = await _context.Users.AnyAsync(u => u.Uid == feedback.Uid);
        if (!userExists)
            return NotFound("User not found");

        // Ensure required Message field
        if (string.IsNullOrWhiteSpace(feedback.Message))
            return BadRequest("Message is required");

        feedback.Date = DateTime.Now; // ensure server sets the date

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Feedback submitted successfully" });
    }

        [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _feedbackService.DeleteFeedback(id);
        return NoContent();
    }
}
