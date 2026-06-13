using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController(AppDbContext context, FeedbackService feedbackService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllFeedbacks()
    {
        IEnumerable<Feedback> feedbacks = await feedbackService.GetAllFeedbacks();
        return Ok(feedbacks);
    }

    [HttpPost]
    public async Task<IActionResult> PostFeedback([FromBody] Feedback feedback)
    {
        // Optional: ensure the user exists
        bool userExists = await context.Users.AnyAsync(u => u.Uid == feedback.Uid);
        if (!userExists)
            return NotFound("User not found");

        // Ensure required Message field
        if (string.IsNullOrWhiteSpace(feedback.Message))
            return BadRequest("Message is required");

        feedback.Date = DateTime.Now; // ensure server sets the date

        context.Feedbacks.Add(feedback);
        await context.SaveChangesAsync();

        return Ok(new { message = "Feedback submitted successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await feedbackService.DeleteFeedback(id);
        return NoContent();
    }
}