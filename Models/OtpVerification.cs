using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Models
{
    public enum OtpType
    {
        EmailVerification = 0,
        PasswordReset = 1,
        Login = 2
    }

    [Table("OtpVerifications")]
    public class OtpVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Identifier { get; set; } = null!; // Email hoặc Phone

        [Required, StringLength(50)]
        public string Code { get; set; } = null!;

        public OtpType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        // Security: Số lần thử nhập sai
        public int FailedAttempts { get; set; } = 0;

        // Security: Thời điểm thử lần cuối
        public DateTime? LastAttemptAt { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    }
}
