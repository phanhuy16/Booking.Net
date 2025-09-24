using BookingApp.Models;

namespace BookingApp.Interface.IService
{
    public interface ITokenService
    {
        string GenerateToken(AppUser appUser, IList<string> role);
        Task<RefreshToken> GenerateAsync(AppUser appUser, string ipAddress, int daysToExpire = 7);
        Task RevokeAsync(RefreshToken token, string ipAddress, string replacedByToken = null!);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
    }
}
