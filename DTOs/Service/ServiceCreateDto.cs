using BookingApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.DTOs.Service
{
    public class ServiceCreateDto
    {
        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        [Range(0, 999999)]
        public decimal Price { get; set; }
        [Range(1, 1440)]
        public int DurationInMinutes { get; set; }
        public ServiceStatus Status { get; set; } = ServiceStatus.Active;
    }
}
