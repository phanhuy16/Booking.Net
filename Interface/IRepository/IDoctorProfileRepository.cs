using BookingApp.Models;

namespace BookingApp.Interface.IRepository
{
    public interface IDoctorProfileRepository
    {
        Task<IEnumerable<DoctorProfile>> GetAllAsync(bool includeDetails = false);
        Task<DoctorProfile?> GetByIdAsync(int id, bool includeDetails = false);
        Task AddAsync(DoctorProfile doctor);
        Task UpdateAsync(DoctorProfile doctor);
        Task DeleteAsync(DoctorProfile doctor);
    }
}
