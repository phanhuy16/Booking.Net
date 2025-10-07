namespace BookingApp.DTOs.DoctorProfile
{
    public class DoctorInSpecialtyDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Workplace { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public double AverageRating { get; set; }
        public int TotalFeedbacks { get; set; }
    }
}
