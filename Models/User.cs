using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class User
{
    [Key]
    public int Uid { get; set; }
    public string Uname { get; set; } = string.Empty;
    public string Upass { get; set; } = string.Empty;

    // Navigation property
    [JsonIgnore]
    public ICollection<Customer>? Customers { get; set; }
}
