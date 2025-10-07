namespace BookingApp.DTOs.Booking
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int ServiceId { get; set; }
        public int ScheduleId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal ServicePrice { get; set; }
        public string ServiceTitle { get; set; } = string.Empty;
    }
}
