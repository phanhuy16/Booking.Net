using BookingApp.Models;

namespace BookingApp.Interface.IRepository
{
    public interface IPatientProfileRepository
    {
        Task<IEnumerable<PatientProfile>> GetAllAsync(bool includeDetails = false);
        Task<PatientProfile?> GetByIdAsync(int id, bool includeDetails = false);
        Task AddAsync(PatientProfile patient);
        Task UpdateAsync(PatientProfile patient);
        Task DeleteAsync(PatientProfile patient);
    }
}
