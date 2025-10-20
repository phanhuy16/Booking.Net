using BookingApp.Data;
using BookingApp.DTOs.Auth;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace BookingApp.Services
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpService> _logger;
        private readonly IConfiguration _configuration;

        private const int MAX_OTP_REQUESTS_PER_HOUR = 5;
        private const int MAX_OTP_REQUESTS_PER_DAY = 10;
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int OTP_EXPIRY_MINUTES = 10;
        private const int PASSWORD_RESET_EXPIRY_MINUTES = 30;

        public OtpService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<OtpService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Generate cryptographically secure OTP code
        /// </summary>
        public string GenerateOtpCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            return (randomNumber % 900000 + 100000).ToString();
        }

        /// <summary>
        /// Send OTP via Email only
        /// </summary>
        public async Task<OtpResponseDto> SendOtpAsync(string email, int? userId = null)
        {
            try
            {
                // Rate limiting - per hour
                var recentOtpsHour = await _context.OtpVerifications
                    .Where(o => o.Identifier == email
                             && o.CreatedAt > DateTime.UtcNow.AddHours(-1))
                    .CountAsync();

                if (recentOtpsHour >= MAX_OTP_REQUESTS_PER_HOUR)
                {
                    _logger.LogWarning($"Rate limit exceeded (hourly) for {email}");
                    return new OtpResponseDto
                    {
                        Success = false,
                        Message = $"Too many OTP requests. Please try again after {60 - DateTime.UtcNow.Minute} minutes."
                    };
                }

                // Rate limiting - per day
                var recentOtpsDay = await _context.OtpVerifications
                    .Where(o => o.Identifier == email
                             && o.CreatedAt > DateTime.UtcNow.AddHours(-24))
                    .CountAsync();

                if (recentOtpsDay >= MAX_OTP_REQUESTS_PER_DAY)
                {
                    _logger.LogWarning($"Rate limit exceeded (daily) for {email}");
                    return new OtpResponseDto
                    {
                        Success = false,
                        Message = "Too many OTP requests today. Please try again tomorrow."
                    };
                }

                // Check for recent OTP (prevent spam within 1 minute)
                var lastOtp = await _context.OtpVerifications
                    .Where(o => o.Identifier == email)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastOtp != null && lastOtp.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
                {
                    var waitSeconds = 60 - (int)(DateTime.UtcNow - lastOtp.CreatedAt).TotalSeconds;
                    return new OtpResponseDto
                    {
                        Success = false,
                        Message = $"Please wait {waitSeconds} seconds before requesting a new OTP."
                    };
                }

                // Invalidate old unused OTPs
                var oldOtps = await _context.OtpVerifications
                    .Where(o => o.Identifier == email && !o.IsUsed)
                    .ToListAsync();

                foreach (var old in oldOtps)
                {
                    old.IsUsed = true;
                }

                // Generate OTP code
                var code = GenerateOtpCode();

                var otp = new OtpVerification
                {
                    Identifier = email,
                    Code = code,
                    Type = OtpType.EmailVerification,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
                    UserId = userId,
                    FailedAttempts = 0
                };

                _context.OtpVerifications.Add(otp);
                await _context.SaveChangesAsync();

                // Send OTP email
                var sent = await _emailService.SendEmailAsync(
                    email,
                    "Email Verification Code",
                    $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #007bff; text-align: center;'>Email Verification</h2>
                        <p>Thank you for registering! Please use the verification code below:</p>
                        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px;'>
                            <p style='font-size: 32px; font-weight: bold; color: #007bff; margin: 0; letter-spacing: 8px;'>{code}</p>
                        </div>
                        <p>This code will expire in <strong>{OTP_EXPIRY_MINUTES} minutes</strong>.</p>
                        <p style='color: #dc3545; font-weight: bold;'>⚠️ Do not share this code with anyone!</p>
                        <p style='color: #6c757d; font-size: 14px;'>If you didn't request this code, please ignore this email.</p>
                    </div>
                    ");

                if (!sent)
                {
                    // Rollback - mark as used if sending failed
                    otp.IsUsed = true;
                    await _context.SaveChangesAsync();

                    return new OtpResponseDto
                    {
                        Success = false,
                        Message = "Failed to send OTP. Please try again."
                    };
                }

                _logger.LogInformation($"OTP sent to {email}");

                return new OtpResponseDto
                {
                    Success = true,
                    Message = "Verification code has been sent to your email."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending OTP to {email}");
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "An error occurred. Please try again later."
                };
            }
        }

        /// <summary>
        /// Verify OTP code
        /// </summary>
        public async Task<bool> VerifyOtpAsync(string email, string code)
        {
            try
            {
                var otp = await _context.OtpVerifications
                    .Where(o => o.Identifier == email && !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    _logger.LogWarning($"No OTP found for {email}");
                    return false;
                }

                // Check failed attempts
                if (otp.FailedAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    _logger.LogWarning($"Too many failed OTP attempts for {email}");
                    otp.IsUsed = true;
                    await _context.SaveChangesAsync();
                    return false;
                }

                // Check expiration
                if (otp.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Expired OTP for {email}");
                    return false;
                }

                // Verify code
                bool isValid = otp.Code == code;

                if (!isValid)
                {
                    otp.FailedAttempts++;
                    otp.LastAttemptAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogWarning(
                        $"Invalid OTP code for {email}. Attempts: {otp.FailedAttempts}/{MAX_FAILED_ATTEMPTS}"
                    );
                    return false;
                }

                // Success - mark as used
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"OTP verified successfully for {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying OTP for {email}");
                return false;
            }
        }

        /// <summary>
        /// Send password reset OTP
        /// </summary>
        public async Task<OtpResponseDto> SendPasswordResetOtpAsync(string email, int userId)
        {
            try
            {
                var recentResets = await _context.OtpVerifications
                    .Where(o => o.Identifier == email
                             && o.Type == OtpType.PasswordReset
                             && o.CreatedAt > DateTime.UtcNow.AddHours(-1))
                    .CountAsync();

                if (recentResets >= 3)
                {
                    _logger.LogWarning($"Too many password reset requests for {email}");
                    return new OtpResponseDto
                    {
                        Success = false,
                        Message = "Too many password reset requests. Please try again later."
                    };
                }

                var oldTokens = await _context.OtpVerifications
                    .Where(o => o.Identifier == email
                             && o.Type == OtpType.PasswordReset
                             && !o.IsUsed)
                    .ToListAsync();

                foreach (var old in oldTokens)
                {
                    old.IsUsed = true;
                }

                var code = GenerateOtpCode();
                var otp = new OtpVerification
                {
                    Identifier = email,
                    Code = code,
                    Type = OtpType.PasswordReset,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(PASSWORD_RESET_EXPIRY_MINUTES),
                    UserId = userId,
                    FailedAttempts = 0
                };

                _context.OtpVerifications.Add(otp);
                await _context.SaveChangesAsync();

                var sent = await _emailService.SendEmailAsync(
                    email,
                    "Password Reset Code",
                    $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #dc3545; text-align: center;'>Password Reset Request</h2>
                        <p>You requested to reset your password.</p>
                        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px;'>
                            <p style='font-size: 32px; font-weight: bold; color: #dc3545; margin: 0; letter-spacing: 8px;'>{code}</p>
                        </div>
                        <p>This code will expire in <strong>{PASSWORD_RESET_EXPIRY_MINUTES} minutes</strong>.</p>
                        <p style='color: #dc3545; font-weight: bold;'>⚠️ If you didn't request this, please ignore this email and secure your account.</p>
                    </div>
                    ");

                if (!sent)
                {
                    return new OtpResponseDto
                    {
                        Success = false,
                        Message = "Failed to send reset code. Please try again."
                    };
                }

                _logger.LogInformation($"Password reset code sent to {email}");

                return new OtpResponseDto
                {
                    Success = true,
                    Message = "Password reset code has been sent to your email."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending password reset OTP to {email}");
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "An error occurred. Please try again later."
                };
            }
        }

        /// <summary>
        /// Get remaining attempts for OTP verification
        /// </summary>
        public async Task<int> GetRemainingAttemptsAsync(string email)
        {
            var otp = await _context.OtpVerifications
                .Where(o => o.Identifier == email && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null) return 0;

            return Math.Max(0, MAX_FAILED_ATTEMPTS - otp.FailedAttempts);
        }

        /// <summary>
        /// Cleanup expired and used OTPs
        /// </summary>
        public async Task<bool> CleanupExpiredOtpsAsync()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-7);

                var expiredOtps = await _context.OtpVerifications
                    .Where(o => o.CreatedAt < cutoffDate
                             || (o.IsUsed && o.UsedAt < cutoffDate)
                             || (o.ExpiresAt < DateTime.UtcNow && !o.IsUsed))
                    .ToListAsync();

                _context.OtpVerifications.RemoveRange(expiredOtps);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cleaned up {expiredOtps.Count} expired OTP records");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired OTPs");
                return false;
            }
        }
    }
}
