using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    public int Oid { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int UnitPrice { get; set; } // Τιμή του προϊόντος κατά την παραγγελία

    // Navigation properties
    [ForeignKey("Oid")]
    [JsonIgnore]
    public Order? Order { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}
