using BookingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Payment
{
    public class PaymentCreateDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    }
}
