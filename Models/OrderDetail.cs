using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class OrderDetail
{
    public int Oid { get; set; }

    public int Pid { get; set; }

    public int Quantity { get; set; }

    // Navigation properties
    [ForeignKey("Oid")]
    // [JsonIgnore]
    public Order? Order { get; set; }

    [ForeignKey("Pid")]
    // [JsonIgnore]
    public Product? Product { get; set; }
}