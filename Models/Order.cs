using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Order
{
    [Key]
    public int Oid { get; set; }

    public int UserId { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [JsonIgnore]
    public ICollection<OrderItem>? OrderItems { get; set; }
}
