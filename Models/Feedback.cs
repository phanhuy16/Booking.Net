using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    [Table("Feedback")]
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DoctorProfile")]
        public int DoctorId { get; set; }

        [ForeignKey("PatientProfile")]
        public int PatientId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 sao

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public DoctorProfile DoctorProfile { get; set; } = null!;
        public PatientProfile PatientProfile { get; set; } = null!;
    }
}
