using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Notification
{
    public class NotificationCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required, StringLength(1000)]
        public string Message { get; set; } = string.Empty;
    }
}
