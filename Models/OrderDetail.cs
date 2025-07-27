using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class OrderDetail
{

    public int Oid { get; set; }

    public int Pid { get; set; }

    public int Quantity { get; set; }

    // Navigation properties
    [ForeignKey("Oid")]
    [JsonIgnore]
    public Order? Order { get; set; }

    [ForeignKey("Pid")]
    [JsonIgnore]
    public Product? Product { get; set; }
}
