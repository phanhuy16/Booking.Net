using BookingApp.DTOs.Auth;

namespace BookingApp.Interface.IService
{
    public interface IAuthService
    {
        // Registration
        Task<OtpResponseDto> RegisterPatientAsync(PatientRegisterDto registerDto);
        Task<OtpResponseDto> RegisterDoctorAsync(DoctorRegisterDto registerDto);

        // Verification
        Task<ResponseDto> VerifyEmailAsync(VerifyAccountDto verifyDto);

        // Login
        Task<ResponseDto> LoginAsync(LoginDto loginDto);
        Task<ResponseDto> GoogleLoginAsync(GoogleLoginDto googleLoginDto);
        Task<ResponseDto> FacebookLoginAsync(FacebookLoginDto facebookLoginDto);
        Task<ResponseDto> RefreshTokenAsync(string token, string ipAddress);

        // OTP
        Task<OtpResponseDto> SendOtpAsync(SendOtpDto sendOtpDto);
        Task<bool> VerifyOtpAsync(VerifyAccountDto verifyDto);
        Task<OtpResponseDto> ResendVerificationAsync(string email);

        // Password Reset
        Task<OtpResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
