using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, MinLength(3)]
        public string FullName { get; set; } = null!;
        [Required, MinLength(3)]
        public string UserName { get; set; } = null!;
        [EmailAddress, Required]
        public string Email { get; set; } = null!;
        [Required, MinLength(6)]
        public string Password { get; set; } = null!;
        [Compare("Password"), Required, MinLength(6)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
