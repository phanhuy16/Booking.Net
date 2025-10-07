using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Schedule
{
    public class ScheduleCreateDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
