using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
    }
}
