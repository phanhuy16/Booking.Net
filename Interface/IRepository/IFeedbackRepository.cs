using BookingApp.Models;

namespace BookingApp.Interface.IRepository
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> GetByDoctorIdAsync(int doctorId, int page, int pageSize);
        Task<int> CountByDoctorIdAsync(int doctorId);
        Task<Feedback?> GetByIdAsync(int id);
        Task AddAsync(Feedback feedback);
        Task UpdateAsync(Feedback feedback);
        Task DeleteAsync(Feedback feedback);
    }
}
