using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }

    [Table("Booking")]
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("PatientProfile")]
        public int PatientId { get; set; }
        [ForeignKey("DoctorProfile")]
        public int DoctorId { get; set; }
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        [ForeignKey("Schedule")]
        public int ScheduleId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // relationships
        public PatientProfile PatientProfile { get; set; } = null!;
        public DoctorProfile DoctorProfile { get; set; } = null!;
        public Service Service { get; set; } = null!;
        public Schedule Schedule { get; set; } = null!;
    }
}
