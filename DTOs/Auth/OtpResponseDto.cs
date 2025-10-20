namespace BookingApp.DTOs.Auth
{
    public class OtpResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? TempToken { get; set; } // Token tạm để verify OTP
    }
}
