using BookingApp.DTOs.Specialty;

namespace BookingApp.DTOs.DoctorProfile
{
    public class DoctorProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public int SpecialtyId { get; set; } // Thêm lại để Edit form hoạt động
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Workplace { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public string? AvatarUrl { get; set; } // Thay đổi: Trả về URL thay vì IFormFile
        public decimal ConsultationFee { get; set; }
        public SpecialtyDto? Specialty { get; set; }
    }
}
