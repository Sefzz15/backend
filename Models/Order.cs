using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Order
{
    [Key]
    public int Oid { get; set; }

    public int Uid { get; set; }

    public DateTime Date { get; set; }

    // Navigation property
    [ForeignKey("Uid")]
    public User? User { get; set; }

    [JsonIgnore]
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}
