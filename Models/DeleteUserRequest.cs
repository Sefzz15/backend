namespace backend.Models;

/// <summary>Body for DELETE /api/users/{id}.</summary>
public class DeleteUserRequest
{
    /// <summary>The target account's own password - the caller must know it.</summary>
    public required string Password { get; set; }
}
