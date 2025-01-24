using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Customer
{
    [Key]
    public int Cid { get; set; }
    public int Uid { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Navigation properties
    [JsonIgnore]
    public User? User { get; set; }
    [JsonIgnore]
    public ICollection<Order>? Orders { get; set; }
}