namespace backend.Models;

/// <summary>Body for PUT /api/users/{id}/password.</summary>
public class UpdatePasswordRequest
{
    /// <summary>The account's current password - the caller must know it.</summary>
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
