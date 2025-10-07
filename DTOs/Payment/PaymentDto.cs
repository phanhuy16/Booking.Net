using BookingApp.Models;

namespace BookingApp.DTOs.Payment
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaidAt { get; set; }
    }
}
