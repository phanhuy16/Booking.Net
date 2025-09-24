using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookingApp.Models
{
    public class AppUser : IdentityUser<int>  
    {
        [Required, StringLength(255)]
        public string FullName { get; set; } = string.Empty;
        public DateTime DateJoined { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        // relationships
        public DoctorProfile? DoctorProfile { get; set; }
        public PatientProfile? PatientProfile { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
