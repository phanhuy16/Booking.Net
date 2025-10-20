using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class VerifyAccountDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!; // Changed from Identifier to Email

        [Required, StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = null!;
    }
}
