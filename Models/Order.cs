using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Order
{
    [Key]
    public int Oid { get; set; }
    public int Cid { get; set; }
    public int Pid { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Customer? Customer { get; set; }
    [JsonIgnore]
    public Product? Product { get; set; }
}