using BookingApp.DTOs.MedicalRecord;

namespace BookingApp.Interface.IService
{
    public interface IMedicalRecordService
    {
        Task<IEnumerable<MedicalRecordDto>> GetAllAsync();
        Task<IEnumerable<MedicalRecordDto>> GetByPatientAsync(int patientUserId);
        Task<MedicalRecordDto?> GetByIdAsync(int id, int currentUserId, string role);
        Task<MedicalRecordDto> CreateAsync(MedicalRecordCreateDto dto, int doctorUserId, IFormFile? attachment);
        Task<bool> DeleteAsync(int id);
    }
}
