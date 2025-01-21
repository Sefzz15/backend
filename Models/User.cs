using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User
    {
        [Key]
        public int uid { get; set; }

        [Required]
        [MaxLength(50)]
        public string uname { get; set; }

        [Required]
        public string upass { get; set; }
    }
}