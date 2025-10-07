namespace BookingApp.DTOs.Booking
{
    public class BookingCreateDto
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int ServiceId { get; set; }
        public int ScheduleId { get; set; }
    }
}
