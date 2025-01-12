using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class User
    {
        public int uid { get; set; }

        [Required]
        public string uname { get; set; } = "";

        [Required]
        public string upass { get; set; } = "";
    }
}