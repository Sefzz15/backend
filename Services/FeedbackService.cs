using Microsoft.EntityFrameworkCore;

public class FeedbackService
{
    private readonly AppDbContext _context;

    public FeedbackService(AppDbContext context)
    {
        _context = context;
    }

    // Get all feedbacks
    public async Task<IEnumerable<Feedback>> GetAllFeedbacks()
    {
        return await _context.Feedbacks
                             .Include(f => f.User)
                             .ToListAsync();
    }

    // Get feedback by ID
    public async Task<Feedback?> GetFeedbackById(int id)
    {
        return await _context.Feedbacks
                             .Include(f => f.User)
                             .FirstOrDefaultAsync(f => f.Fid == id);
    }

    // Add new feedback
    public async Task AddFeedback(Feedback feedback)
    {
        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();
    }

    // Update existing feedback
    public async Task UpdateFeedback(Feedback feedback)
    {
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync();
    }

    // Delete feedback by ID
    public async Task DeleteFeedback(int id)
    {
        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback != null)
        {
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
        }
    }

    // Get feedbacks by user ID
    public async Task<IEnumerable<Feedback>> GetFeedbacksByUserId(int userId)
    {
        return await _context.Feedbacks
                             .Include(f => f.User)
                             .Where(f => f.Uid == userId)
                             .ToListAsync();
    }
}
