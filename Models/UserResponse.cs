namespace backend.Models;

public class UserResponse
{
    public int Uid { get; set; }
    public string Uname { get; set; } = string.Empty;
    public string Upass { get; set; } = string.Empty;

    public static UserResponse From(User user) =>
        new() { Uid = user.Uid, Uname = user.Uname, Upass = user.Upass };
}
