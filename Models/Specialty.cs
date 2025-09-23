using System.ComponentModel.DataAnnotations;

namespace BookingApp.Models
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public ICollection<DoctorProfile> DoctorProfiles { get; set; } = new List<DoctorProfile>();
    }
}
