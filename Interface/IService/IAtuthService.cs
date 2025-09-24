using BookingApp.DTOs.Auth;

namespace BookingApp.Interface.IService
{
    public interface IAuthService
    {
        Task<ResponceDto> RegisterAsync(RegisterDto registerDto);
        Task<ResponceDto> LoginAsync(LoginDto loginDto);
        Task<ResponceDto> RefreshTokenAsync(string token, string ipAddress);
    }
}
