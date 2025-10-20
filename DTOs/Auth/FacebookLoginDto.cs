using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Auth
{
    public class FacebookLoginDto
    {
        [Required]
        public string AccessToken { get; set; } = null!;
    }
}
