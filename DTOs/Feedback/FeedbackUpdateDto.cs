using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Feedback
{
    public class FeedbackUpdateDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
    }
}
