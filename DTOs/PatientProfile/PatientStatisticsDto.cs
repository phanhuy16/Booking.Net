namespace BookingApp.DTOs.PatientProfile
{
    public class PatientStatisticsDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public List<PatientMonthlyStatisticDto> Statistics { get; set; } = new();
    }

    public class PatientMonthlyStatisticDto
    {
        public string Month { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public double? AverageRating { get; set; } // nullable nếu không có feedback
    }
}
