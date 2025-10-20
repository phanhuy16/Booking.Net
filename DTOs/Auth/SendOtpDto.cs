using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class SendOtpDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
    }
}
