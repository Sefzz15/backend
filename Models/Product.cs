using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Product
    {   [Key]
        public int pid { get; set; }

        [Required]
        [MaxLength(50)]
        public string p_name { get; set; }

        [MaxLength(100)]
        public string? description { get; set; }

        [Required]
        public decimal price { get; set; }

        [Required]
        public int stock { get; set; }
    }
}