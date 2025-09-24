using Azure.Core;
using BookingApp.DTOs.Auth;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.AspNetCore.Identity;

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

        public AuthService(ILogger<AuthService> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IHttpContextAccessor httpContextAccessor,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // Implement Register and Login methods here
        public async Task<ResponceDto> RegisterAsync(RegisterDto registerDto)
        {

            var user = new AppUser
            {
                UserName = registerDto.UserName,
                FullName = registerDto.FullName,
                Email = registerDto.Email,
            };

            var createResult = await _userManager.CreateAsync(user, registerDto.Password);

            if (createResult.Succeeded)
            {
                var role = await _userManager.AddToRoleAsync(user, "Patient");
                if (role.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var ipAddress = GetIpAddress();
                    var accessToken = _tokenService.GenerateToken(user, roles);
                    var refreshToken = await _tokenService.GenerateAsync(user, ipAddress);

                    return new ResponceDto { Success = true, UserName = user.UserName, AccessToken = accessToken, RefreshToken = refreshToken.Token };
                }
                else
                {
                    return new ResponceDto { Success = false, Errors = new[] { "Role does not exist" } };
                }
            }
            else
            {
                return new ResponceDto { Success = false, Errors = createResult.Errors.Select(e => e.Description) };
            }
        }

        public async Task<ResponceDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                return new ResponceDto { Success = false, Errors = new[] { "User not found" } };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
                return new ResponceDto { Success = false, Errors = new[] { "Invalid credentials" } };

            var ipAddress = GetIpAddress();
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateToken(user, roles);
            var refreshToken = await _tokenService.GenerateAsync(user, ipAddress);

            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new ResponceDto
            {
                Success = true,
                UserName = user.UserName!,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<ResponceDto> RefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync(token);

            if (refreshToken == null)
                return new ResponceDto { Success = false, Errors = new[] { "Invalid refresh token." } };

            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user == null)
                return new ResponceDto { Success = false, Errors = new[] { "User not found." } };

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateToken(user, roles);
            var newRefreshToken = await _tokenService.GenerateAsync(user, ipAddress);

            await _tokenService.RevokeAsync(refreshToken, ipAddress, newRefreshToken.Token);

            return new ResponceDto
            {
                Success = true,
                UserName = user.UserName!,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        private string GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.ContainsKey("X-Forwarded-For"))
                return context.Request.Headers["X-Forwarded-For"]!;
            else
                return context?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
        }
    }
}
