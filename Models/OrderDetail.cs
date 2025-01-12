using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class OrderDetail
    {
        public int o_details_id { get; set; }

        [ForeignKey(nameof(Order))]
        public int o_id { get; set; }

        [ForeignKey(nameof(Product))]
        public int p_id { get; set; }

        public int quantity { get; set; }

        public decimal price { get; set; }

        // Navigation Properties
        public required Order order { get; set; }

        public required Product product { get; set; }
    }
}