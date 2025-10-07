using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2
    }

    [Table("PatientProfile")]
    public class PatientProfile
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AppUser")]
        public int UserId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; } 
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        // relationships
        public AppUser User { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
