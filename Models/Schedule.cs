using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    [Table("Schedule")]
    public class Schedule
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("DoctorProfile")]
        public int DoctorId { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;

        // relationships
        public DoctorProfile DoctorProfile { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
