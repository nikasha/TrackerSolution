using System.ComponentModel.DataAnnotations;

namespace StorageService.Models
{
    public class Visit
    {
        public int Id { get; set; }
        public string? Referer { get; set; }
        public string? UserAgent { get; set; }

        [Required]
        public required string IP { get; set; }
        public DateTime VisitTime { get; set; } = DateTime.UtcNow;
    }
}