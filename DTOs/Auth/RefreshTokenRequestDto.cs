using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
