namespace BookingApp.DTOs.PatientProfile
{
    public class PatientDashboardDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int TotalMedicalRecords { get; set; }

        public List<UpcomingBookingDto> UpcomingBookings { get; set; } = new();
        public LatestFeedbackDto? LatestFeedback { get; set; }
    }

    public class UpcomingBookingDto
    {
        public int BookingId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class LatestFeedbackDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
