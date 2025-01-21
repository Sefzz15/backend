using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class OrderDetail
    {
        [Key]
        public int o_details_id { get; set; }

        [ForeignKey(nameof(Order))]
        public int oid { get; set; }

        [ForeignKey(nameof(Product))]
        public int pid { get; set; }

        [MaxLength(5)]
        public int quantity { get; set; }

        [MaxLength(10)]
        public decimal price { get; set; }

        [Required]
        public Order order { get; set; }

        [Required]
        public Product product { get; set; }
    }
}