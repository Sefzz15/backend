using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Feedback
{
    [Key]
    public int Fid { get; set; }

    public int Uid { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    // Navigation property
    [ForeignKey("Uid")]
    public User? User { get; set; }
}
