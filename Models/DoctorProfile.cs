using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    [Table("DoctorProfile")]
    public class DoctorProfile
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AppUser")]
        public int UserId { get; set; }
        [ForeignKey("Specialty")]
        public int SpecialtyId { get; set; }
        [Range(0, 100)]
        public int ExperienceYears { get; set; }
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        [StringLength(200)]
        public string Workplace { get; set; } = string.Empty;

        // relationships
        public AppUser User { get; set; } = null!;
        public Specialty Specialty { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
