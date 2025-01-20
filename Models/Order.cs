using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Order
    {
        [Key]
        public int oid { get; set; }

        [ForeignKey(nameof(Customer))]
        public int cid { get; set; }

        public DateTime o_date { get; set; }

        public decimal total_amount { get; set; }

        [Required]
        public Customer customer { get; set; }

        [Required]
        public List<OrderDetail> order_details { get; } = new();
    }
}