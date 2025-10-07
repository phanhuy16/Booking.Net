using BookingApp.Models;

namespace BookingApp.Interface.IRepository
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllByUserIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int id);
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(Notification notification);
        Task SaveAsync();
    }
}
