using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class DoctorRegisterDto
    {
        [Required, MinLength(3)]
        public string Name { get; set; } = null!;

        [EmailAddress, Required]
        public string Email { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;

        // Additional doctor-specific fields
        public int? SpecialtyId { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Workplace { get; set; }
        public string? Description { get; set; }
    }
}
