using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookingApp.Models
{
    public class AppUser : IdentityUser<int>  
    {
        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        // relationships
        public ICollection<Notification>? Notifications { get; set; }
        public DoctorProfile? DoctorProfile { get; set; }
        public PatientProfile? PatientProfile { get; set; }
    }
}
