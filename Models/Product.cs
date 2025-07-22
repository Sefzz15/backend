using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Product
{
    [Key]
    public int Pid { get; set; }
    public string Pname { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    // Navigation property
    // [JsonIgnore]
    // public ICollection<Order>? Orders { get; set; }

    // Navigation property for order items
    [JsonIgnore]
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}