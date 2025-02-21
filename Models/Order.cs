using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Order
{
    [Key]
    public int Oid { get; set; } // Primary key

        public int Ooid { get; set; }

    public int Cid { get; set; } // Customer ID
    public DateTime Date { get; set; }

      // Navigation property for order items
  
   [JsonIgnore]
    public Customer? Customer { get; set; }
    [JsonIgnore]
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}