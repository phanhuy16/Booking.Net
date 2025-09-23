using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        Insurance,
        Online
    }

    [Table("Payment")]
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [StringLength(50)]
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } 
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // relationships
        public Booking Booking { get; set; } = null!;
    }
}
