using BookingApp.DTOs.PatientProfile;

namespace BookingApp.Interface.IService
{
    public interface IPatientProfileService
    {
        Task<IEnumerable<PatientProfileDto>> GetAllAsync();
        Task<PatientProfileWithDetailsDto?> GetByIdAsync(int id);
        Task<PatientDashboardDto?> GetDashboardAsync(int patientId);
        Task<PatientStatisticsDto?> GetStatisticsAsync(int patientId, DateTime from, DateTime to);
        Task<PatientProfileDto> CreateAsync(PatientProfileCreateDto dto);
        Task<bool> UpdateAsync(int id, PatientProfileUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
