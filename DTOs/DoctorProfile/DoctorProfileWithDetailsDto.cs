namespace BookingApp.DTOs.DoctorProfile
{
    public class DoctorProfileWithDetailsDto : DoctorProfileDto
    {
        public List<DoctorScheduleDto> AvailableSchedules { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalFeedbacks { get; set; }
    }
}
