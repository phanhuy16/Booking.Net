using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AppUser")]
        public int UserId { get; set; }
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // relationships
        public AppUser User { get; set; } = null!;
    }
}
