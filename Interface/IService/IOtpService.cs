using BookingApp.DTOs.Auth;

namespace BookingApp.Interface.IService
{
    public interface IOtpService
    {
        /// <summary>
        /// Generate cryptographically secure 6-digit OTP code
        /// </summary>
        string GenerateOtpCode();

        /// <summary>
        /// Send OTP to email or phone with rate limiting
        /// </summary>
        Task<OtpResponseDto> SendOtpAsync(string email, int? userId = null);

        /// <summary>
        /// Verify OTP code with failed attempts tracking
        /// </summary>
        Task<bool> VerifyOtpAsync(string identifier, string code);

        /// <summary>
        /// Send password reset OTP (longer expiry time)
        /// </summary>
        Task<OtpResponseDto> SendPasswordResetOtpAsync(string email, int userId);

        /// <summary>
        /// Get remaining verification attempts for an OTP
        /// </summary>
        Task<int> GetRemainingAttemptsAsync(string identifier);

        /// <summary>
        /// Cleanup expired and old OTP records
        /// </summary>
        Task<bool> CleanupExpiredOtpsAsync();
    }
}
