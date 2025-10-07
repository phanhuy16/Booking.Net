namespace BookingApp.DTOs.PatientProfile
{
    public class PatientProfileWithDetailsDto : PatientProfileDto
    {
        public List<PatientBookingDto> Bookings { get; set; } = new();
        public List<PatientMedicalRecordDto> MedicalRecords { get; set; } = new();
    }

    public class PatientBookingDto
    {
        public int BookingId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PatientMedicalRecordDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
    }
}
