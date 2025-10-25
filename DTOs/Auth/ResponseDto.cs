namespace BookingApp.DTOs.Auth
{
    public class ResponseDto
    {
        public bool Success { get; set; }
        public string UserName { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; set; }

        // ⭐ Thêm các field mới
        public int? PatientProfileId { get; set; }
        public int? DoctorProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
