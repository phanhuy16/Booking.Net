namespace BookingApp.DTOs.DoctorProfile
{
    public class DoctorScheduleDto
    {
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
