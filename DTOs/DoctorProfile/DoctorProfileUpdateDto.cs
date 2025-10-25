using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.DoctorProfile
{
    public class DoctorProfileUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; } // Null = dùng "Doctor@123"

        [Required]
        public int SpecialtyId { get; set; }

        [Range(0, 100)]
        public int ExperienceYears { get; set; }

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string Workplace { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }

        public IFormFile? Avatar { get; set; }
    }
}
