using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class PatientRegisterDto
    {
        [Required, MinLength(3)]
        public string FullName { get; set; } = null!;

        [EmailAddress, Required]
        public string Email { get; set; } = null!;

        [Phone]
        public string? Phone { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;
    }
}
