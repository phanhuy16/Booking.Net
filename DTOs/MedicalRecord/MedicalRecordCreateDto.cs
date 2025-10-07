using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.MedicalRecord
{
    public class MedicalRecordCreateDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required, StringLength(1000)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Prescription { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
