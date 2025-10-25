using Azure.Core;
using BookingApp.Data;
using BookingApp.DTOs.Auth;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Web;

namespace BookingApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
              ILogger<AuthService> logger,
              UserManager<AppUser> userManager,
              SignInManager<AppUser> signInManager,
              RoleManager<IdentityRole<int>> roleManager,
              IHttpContextAccessor httpContextAccessor,
              ITokenService tokenService,
              IOtpService otpService,
              IEmailService emailService,
              ApplicationDbContext context,
              IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _otpService = otpService;
            _emailService = emailService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        // ===== PATIENT REGISTRATION (Email với OTP) =====
        public async Task<OtpResponseDto> RegisterPatientAsync(PatientRegisterDto registerDto)
        {
            // Kiểm tra email đã tồn tại
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            // Kiểm tra phone nếu có
            if (!string.IsNullOrEmpty(registerDto.Phone))
            {
                var existingPhone = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == registerDto.Phone);
                if (existingPhone != null)
                {
                    return new OtpResponseDto
                    {
                        Success = false,
                        Message = "Phone number already exists"
                    };
                }
            }

            // Tạo user với email làm username
            var user = new AppUser
            {
                UserName = registerDto.Email,
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.Phone,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, registerDto.Password);

            if (!createResult.Succeeded)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", createResult.Errors.Select(e => e.Description))
                };
            }

            // Thêm role Patient
            await _userManager.AddToRoleAsync(user, "Patient");

            // Tạo PatientProfile
            var patientProfile = new PatientProfile
            {
                UserId = user.Id
            };
            _context.PatientProfiles.Add(patientProfile);
            await _context.SaveChangesAsync();

            // Gửi OTP qua Email
            var otpResult = await _otpService.SendOtpAsync(registerDto.Email, user.Id);

            if (!otpResult.Success)
            {
                // Rollback user creation
                await _userManager.DeleteAsync(user);
                _context.PatientProfiles.Remove(patientProfile);
                await _context.SaveChangesAsync();
                return otpResult;
            }

            _logger.LogInformation($"Patient registration completed for email: {registerDto.Email}");

            return new OtpResponseDto
            {
                Success = true,
                Message = "Registration successful. Please check your email for verification code.",
                TempToken = GenerateTempToken(user.Id)
            };
        }

        // ===== DOCTOR REGISTRATION (Email với OTP) =====
        public async Task<OtpResponseDto> RegisterDoctorAsync(DoctorRegisterDto registerDto)
        {
            // Kiểm tra email đã tồn tại
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            // Tạo user
            var user = new AppUser
            {
                UserName = registerDto.Email,
                FullName = registerDto.Name,
                Email = registerDto.Email,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, registerDto.Password);

            if (!createResult.Succeeded)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", createResult.Errors.Select(e => e.Description))
                };
            }

            // Thêm role Doctor
            await _userManager.AddToRoleAsync(user, "Doctor");

            // Tạo DoctorProfile
            var doctorProfile = new DoctorProfile
            {
                UserId = user.Id,
                SpecialtyId = registerDto.SpecialtyId ?? 1,
                ExperienceYears = registerDto.ExperienceYears ?? 0,
                Workplace = registerDto.Workplace ?? "",
                Description = registerDto.Description ?? ""
            };

            _context.DoctorProfiles.Add(doctorProfile);
            await _context.SaveChangesAsync();

            // Gửi OTP qua Email
            var otpResult = await _otpService.SendOtpAsync(registerDto.Email, user.Id);

            if (!otpResult.Success)
            {
                // Rollback
                await _userManager.DeleteAsync(user);
                _context.DoctorProfiles.Remove(doctorProfile);
                await _context.SaveChangesAsync();
                return otpResult;
            }

            _logger.LogInformation($"Doctor registration completed for {registerDto.Email}. OTP sent.");

            return new OtpResponseDto
            {
                Success = true,
                Message = "Registration successful. Please check your email for verification code.",
                TempToken = GenerateTempToken(user.Id)
            };
        }

        // ===== VERIFY EMAIL (for both Patient and Doctor) =====
        public async Task<ResponseDto> VerifyEmailAsync(VerifyAccountDto verifyDto)
        {
            // Verify OTP
            var isValid = await _otpService.VerifyOtpAsync(verifyDto.Email, verifyDto.OtpCode);

            if (!isValid)
            {
                var remaining = await _otpService.GetRemainingAttemptsAsync(verifyDto.Email);
                return new ResponseDto
                {
                    Success = false,
                    Errors = new[] { $"Invalid or expired OTP code. {remaining} attempts remaining." }
                };
            }

            var user = await _userManager.Users
                  .Include(u => u.PatientProfile)
                  .Include(u => u.DoctorProfile)
                  .FirstOrDefaultAsync(u => u.Email == verifyDto.Email);

            if (user == null)
            {
                return new ResponseDto
                {
                    Success = false,
                    Errors = new[] { "User not found" }
                };
            }

            // Check if already verified
            if (user.EmailConfirmed)
            {
                return new ResponseDto
                {
                    Success = false,
                    Errors = new[] { "Email already verified. Please login." }
                };
            }

            // Confirm email
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            // Auto-login after verification
            var ipAddress = GetIpAddress();
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateToken(user, roles);
            var refreshToken = await _tokenService.GenerateAsync(user, ipAddress);

            _logger.LogInformation($"User {user.Email} verified and logged in");

            return new ResponseDto
            {
                Success = true,
                UserName = user.UserName!,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles,
                PatientProfileId = user.PatientProfile?.Id,
                DoctorProfileId = user.DoctorProfile?.Id
            };
        }

        // ===== LOGIN =====
        public async Task<ResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.Users
                   .Include(u => u.PatientProfile)
                   .Include(u => u.DoctorProfile)
                   .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                _logger.LogWarning("Login attempt failed. User {Email} not found.", loginDto.Email);
                return new ResponseDto { Success = false, Errors = new[] { "Invalid email or password" } };
            }

            // Check if email is verified
            if (!user.EmailConfirmed)
            {
                return new ResponseDto
                {
                    Success = false,
                    Errors = new[] { "Please verify your email before logging in" }
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, true);

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account {Email} is locked out.", loginDto.Email);
                return new ResponseDto { Success = false, Errors = new[] { "Account is locked. Please try again later." } };
            }

            if (!result.Succeeded)
                return new ResponseDto { Success = false, Errors = new[] { "Invalid email or password" } };

            var roles = await _userManager.GetRolesAsync(user);
            var ipAddress = GetIpAddress();
            var accessToken = _tokenService.GenerateToken(user, roles);
            var refreshToken = await _tokenService.GenerateAsync(user, ipAddress);

            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {Email} logged in successfully.", user.Email);

            return new ResponseDto
            {
                Success = true,
                UserName = user.UserName!,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles,
                PatientProfileId = user.PatientProfile?.Id,
                DoctorProfileId = user.DoctorProfile?.Id
            };
        }

        public async Task<ResponseDto> RefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync(token);

            if (refreshToken == null)
                return new ResponseDto { Success = false, Errors = new[] { "Invalid refresh token." } };

            var user = await _userManager.Users
                   .Include(u => u.PatientProfile)
                   .Include(u => u.DoctorProfile)
                   .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId);
            if (user == null)
                return new ResponseDto { Success = false, Errors = new[] { "User not found." } };

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateToken(user, roles);
            var newRefreshToken = await _tokenService.GenerateAsync(user, ipAddress);

            await _tokenService.RevokeAsync(refreshToken, ipAddress, newRefreshToken.Token);

            return new ResponseDto
            {
                Success = true,
                UserName = user.UserName!,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles,
                PatientProfileId = user.PatientProfile?.Id,
                DoctorProfileId = user.DoctorProfile?.Id
            };
        }

        // ===== SOCIAL LOGIN =====
        public async Task<ResponseDto> GoogleLoginAsync(GoogleLoginDto googleLoginDto)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                googleLoginDto.IdToken,
                new GoogleJsonWebSignature.ValidationSettings()
            );

            if (payload == null)
            {
                return new ResponseDto { Success = false, Errors = new[] { "Invalid Google token." } };
            }

            var user = await _userManager.Users
                .Include(u => u.PatientProfile)
                .Include(u => u.DoctorProfile)
                .FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                // Auto-create patient account for Google login
                user = new AppUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FullName = payload.Name,
                    EmailConfirmed = true // Google already verified
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return new ResponseDto { Success = false, Errors = createResult.Errors.Select(e => e.Description) };
                }

                await _userManager.AddToRoleAsync(user, "Patient");

                // Create patient profile
                var patientProfile = new PatientProfile
                {
                    UserId = user.Id
                };
                _context.PatientProfiles.Add(patientProfile);
                await _context.SaveChangesAsync();

                // Reload user with profile
                user = await _userManager.Users
                    .Include(u => u.PatientProfile)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                _logger.LogInformation($"New patient account created via Google for {payload.Email}");
            }

            var roles = await _userManager.GetRolesAsync(user!);
            var ipAddress = GetIpAddress();
            var accessToken = _tokenService.GenerateToken(user!, roles);
            var refreshToken = await _tokenService.GenerateAsync(user!, ipAddress);

            user!.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new ResponseDto
            {
                Success = true,
                UserName = user.UserName!,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles,
                PatientProfileId = user.PatientProfile?.Id,
                DoctorProfileId = user.DoctorProfile?.Id
            };
        }

        public Task<ResponseDto> FacebookLoginAsync(FacebookLoginDto facebookLoginDto)
        {
            throw new NotImplementedException("Facebook login needs Facebook SDK implementation");
        }

        // ===== OTP METHODS =====
        public async Task<OtpResponseDto> SendOtpAsync(SendOtpDto sendOtpDto)
        {
            return await _otpService.SendOtpAsync(sendOtpDto.Email);
        }

        public async Task<bool> VerifyOtpAsync(VerifyAccountDto verifyDto)
        {
            return await _otpService.VerifyOtpAsync(verifyDto.Email, verifyDto.OtpCode);
        }

        // ===== PASSWORD RESET =====
        public async Task<OtpResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // Don't reveal if user exists
                return new OtpResponseDto
                {
                    Success = true,
                    Message = "If the email exists, a reset code has been sent."
                };
            }

            return await _otpService.SendPasswordResetOtpAsync(forgotPasswordDto.Email, user.Id);
        }

        public async Task<ResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var otpVerification = await _context.OtpVerifications
                .Where(o => o.Code == resetPasswordDto.Token
                            && o.Type == OtpType.PasswordReset
                            && !o.IsUsed
                            && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpVerification == null)
            {
                return new ResponseDto
                {
                    Success = false,
                    Errors = new[] { "Invalid or expired reset code." }
                };
            }

            var user = await _userManager.FindByIdAsync(otpVerification.UserId.ToString()!);
            if (user == null)
            {
                return new ResponseDto
                {
                    Success = false,
                    Errors = new[] { "User not found." }
                };
            }

            // Check failed attempts
            if (otpVerification.FailedAttempts >= 5)
            {
                return new ResponseDto
                {
                    Success = false,
                    Errors = new[] { "Too many failed attempts. Please request a new reset code." }
                };
            }

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                otpVerification.FailedAttempts++;
                await _context.SaveChangesAsync();

                return new ResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }

            // Mark OTP as used
            otpVerification.IsUsed = true;
            otpVerification.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Password reset successfully for user {user.Email}");

            return new ResponseDto
            {
                Success = true,
                UserName = user.UserName!,
                AccessToken = string.Empty,
                RefreshToken = string.Empty
            };
        }

        // ===== RESEND VERIFICATION =====
        public async Task<OtpResponseDto> ResendVerificationAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.EmailConfirmed)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "Invalid request or already verified"
                };
            }

            return await _otpService.SendOtpAsync(email, user.Id);
        }

        // ===== HELPER METHODS =====
        private string GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.ContainsKey("X-Forwarded-For"))
                return context.Request.Headers["X-Forwarded-For"]!;
            else
                return context?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
        }

        private string GenerateTempToken(int userId)
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return $"{userId}_{Convert.ToBase64String(bytes)}";
        }
    }
}
