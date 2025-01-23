using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Customer
    {
        [Key]
        public int cid { get; set; }

        [Required]
        public int uid { get; set; }

        [ForeignKey(nameof(uid))]
        public User? User { get; set; }

        [Required]
        [MaxLength(50)]
        public string first_name { get; set; }

        [MaxLength(50)]
        public string last_name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public string email { get; set; }

        public ICollection<Order> orders { get; set; } = new List<Order>();
    }
}