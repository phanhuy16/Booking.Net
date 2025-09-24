using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;
    }
}
