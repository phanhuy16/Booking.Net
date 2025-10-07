namespace BookingApp.DTOs.DoctorProfile
{
    public class DoctorProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Workplace { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
