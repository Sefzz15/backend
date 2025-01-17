using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Customer
    {
        public int c_id { get; set; }

        [Required]
        public int uid { get; set; }

        [ForeignKey(nameof(uid))]
        public User? User { get; set; }

        [Required]
        [MaxLength(50)]
        public required string first_name { get; set; }

        [MaxLength(50)]
        public required string last_name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public required string email { get; set; }

        [MaxLength(15)]
        public string? phone { get; set; }

        [MaxLength(255)]
        public string? address { get; set; }

        [Required]
        [MaxLength(50)]
        public required string city { get; set; }

        public ICollection<Order> orders { get; set; } = new List<Order>();
    }
}
