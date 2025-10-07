using BookingApp.Models;

namespace BookingApp.Interface.IRepository
{
    public interface IMedicalRecordRepository
    {
        Task<IEnumerable<MedicalRecord>> GetAllAsync();
        Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId);
        Task<MedicalRecord?> GetByIdAsync(int id);
        Task AddAsync(MedicalRecord record);
    }
}
