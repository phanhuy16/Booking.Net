using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Specialty
{
    public class SpecialtyUpdateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public IFormFile? Icon { get; set; } // có thể update icon
    }
}
