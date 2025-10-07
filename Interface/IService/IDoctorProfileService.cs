using BookingApp.DTOs.DoctorProfile;

namespace BookingApp.Interface.IService
{
    public interface IDoctorProfileService
    {
        Task<IEnumerable<DoctorProfileDto>> GetAllAsync(string? specialty = null);
        Task<DoctorProfileDto?> GetByIdAsync(int id);
        Task<DoctorProfileWithDetailsDto?> GetDoctorWithDetailsAsync(int id);
        Task<IEnumerable<DoctorScheduleDto>> GetAvailableSchedulesAsync(int doctorId);
        Task<DoctorProfileDto> CreateAsync(DoctorProfileCreateDto dto);
        Task<DoctorProfileDto> UpdateAsync(int id, DoctorProfileUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
