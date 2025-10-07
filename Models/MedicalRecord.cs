
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    [Table("MedicalRecord")]
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientProfile")]
        public int PatientId { get; set; }

        [ForeignKey("DoctorProfile")]
        public int DoctorId { get; set; }   // bác sĩ nào tạo hồ sơ này

        [ForeignKey("Booking")]
        public int BookingId { get; set; }  // hồ sơ y tế được tạo từ booking nào

        [StringLength(1000)]
        public string Diagnosis { get; set; } = string.Empty;  // chẩn đoán

        [StringLength(2000)]
        public string? Prescription { get; set; }              // đơn thuốc (nếu có)

        [StringLength(1000)]
        public string? Notes { get; set; }                     // ghi chú thêm
        [StringLength(2000)]
        public string? AttachmentUrl { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // relationships
        public PatientProfile PatientProfile { get; set; } = null!;
        public DoctorProfile DoctorProfile { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
    }

}