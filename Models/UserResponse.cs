namespace backend.Models;

/// <summary>User fields that are safe to send to a client. Never the hash.</summary>
public class UserResponse
{
    public int Uid { get; set; }
    public string Uname { get; set; } = string.Empty;

    public static UserResponse From(User user) => new() { Uid = user.Uid, Uname = user.Uname };
}
