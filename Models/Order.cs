using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Order
    {
        public int o_id { get; set; }

        [ForeignKey(nameof(Customer))]
        public int c_id { get; set; }

        public DateTime o_date { get; set; }

        public decimal total_amount { get; set; }

        [Required]
        public required Customer customer { get; set; }

        [Required]
        public ICollection<OrderDetail> order_details { get; set; } = new List<OrderDetail>();
    }
}