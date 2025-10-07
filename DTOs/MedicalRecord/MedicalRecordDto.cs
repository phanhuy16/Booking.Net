namespace BookingApp.DTOs.MedicalRecord
{
    public class MedicalRecordDto
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Prescription { get; set; }
        public string? Notes { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
