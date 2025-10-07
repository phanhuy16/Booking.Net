using BookingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.Payment
{
    public class PaymentUpdateDto
    {
        [Required]
        public PaymentStatus Status { get; set; }
    }
}
