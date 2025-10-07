using BookingApp.Models;

namespace BookingApp.DTOs.Service
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
        public ServiceStatus Status { get; set; }
    }
}
