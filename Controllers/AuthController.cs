using BookingApp.DTOs.Auth;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Authorize]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger,
            SignInManager<AppUser> signInManager)
        {
            _authService = authService;
            _logger = logger;
            _signInManager = signInManager;
        }

        // ===== PATIENT ENDPOINTS =====

        /// <summary>
        /// Register patient with email (OTP verification required)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("patient/register")]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterPatientAsync(registerDto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ===== DOCTOR ENDPOINTS =====

        /// <summary>
        /// Register doctor with email (OTP verification required)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("doctor/register")]
        public async Task<IActionResult> RegisterDoctor([FromBody] DoctorRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterDoctorAsync(registerDto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ===== VERIFICATION ENDPOINT (for both Patient and Doctor) =====

        /// <summary>
        /// Verify email with OTP code (for both Patient and Doctor)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyAccountDto verifyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyEmailAsync(verifyDto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ===== LOGIN ENDPOINT =====

        /// <summary>
        /// Login with email and password (for both Patient and Doctor)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Login with Google (auto-creates Patient account)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login/google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
        {
            var result = await _authService.GoogleLoginAsync(googleLoginDto);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Login with Facebook (not implemented yet)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login/facebook")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginDto facebookLoginDto)
        {
            try
            {
                var result = await _authService.FacebookLoginAsync(facebookLoginDto);
                if (!result.Success)
                    return Unauthorized(result);

                return Ok(result);
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, new { success = false, message = "Facebook login is not implemented yet" });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var result = await _authService.RefreshTokenAsync(refreshTokenRequest.RefreshToken, ipAddress);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        // ===== OTP ENDPOINTS =====

        /// <summary>
        /// Send OTP to email
        /// </summary>
        [AllowAnonymous]
        [HttpPost("otp/send")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto sendOtpDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.SendOtpAsync(sendOtpDto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Verify OTP code
        /// </summary>
        [AllowAnonymous]
        [HttpPost("otp/verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyAccountDto verifyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isValid = await _authService.VerifyOtpAsync(verifyDto);
            if (!isValid)
                return BadRequest(new { success = false, message = "Invalid or expired OTP" });

            return Ok(new { success = true, message = "OTP verified successfully" });
        }

        /// <summary>
        /// Resend verification OTP to email
        /// </summary>
        [AllowAnonymous]
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] SendOtpDto sendOtpDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResendVerificationAsync(sendOtpDto.Email);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ===== PASSWORD RESET =====

        /// <summary>
        /// Request password reset (sends OTP to email)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(result);
        }

        /// <summary>
        /// Reset password using OTP code
        /// </summary>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ===== LOGOUT =====

        /// <summary>
        /// Logout (client should delete tokens)
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { success = true, message = "Logged out successfully" });
        }
    }
}
