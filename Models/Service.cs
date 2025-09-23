using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    public enum ServiceStatus
    {
        Active,
        Inactive
    }

    [Table("Service")]
    public class Service
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Range(1, 1440)] // Duration between 1 minute and 24 hours
        public int DurationInMinutes { get; set; }
        public ServiceStatus Status { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
