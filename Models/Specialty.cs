using System.ComponentModel.DataAnnotations;

namespace BookingApp.Models
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? IconUrl { get; set; } // upload lên Firebase
        public ICollection<DoctorProfile> DoctorProfiles { get; set; } = new List<DoctorProfile>();
    }
}
