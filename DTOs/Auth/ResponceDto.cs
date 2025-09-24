namespace BookingApp.DTOs.Auth
{
    public class ResponceDto
    {
        public bool Success { get; set; }
        public string UserName { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; set; }
    }
}
