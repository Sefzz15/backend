using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Product
{
    [Key]
    public int Pid { get; set; }
    public string Pname { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Stock { get; set; }

    // Navigation property
    [JsonIgnore]
    public ICollection<Order>? Orders { get; set; }
}