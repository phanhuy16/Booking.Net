namespace BookingApp.DTOs.Specialty
{
    public class SpecialtyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int DoctorCount { get; set; }
    }
}
