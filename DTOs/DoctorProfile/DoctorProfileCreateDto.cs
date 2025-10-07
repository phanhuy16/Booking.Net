using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.DoctorProfile
{
    public class DoctorProfileCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int SpecialtyId { get; set; }

        [Range(0, 100)]
        public int ExperienceYears { get; set; }

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string Workplace { get; set; } = string.Empty;
    }
}
