using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Product
    {
        public int p_id { get; set; }

        [Required]
        public string p_name { get; set; } = "";

        public string? description { get; set; }

        [Required]
        public decimal price { get; set; }

        [Required]

        public int stock_quantity { get; set; }
    }
}