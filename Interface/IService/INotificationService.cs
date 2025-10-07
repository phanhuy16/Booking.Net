using BookingApp.DTOs.Notification;

namespace BookingApp.Interface.IService
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAllByUserIdAsync(int userId);
        Task<NotificationDto?> GetByIdAsync(int id);
        Task<NotificationDto> CreateAsync(NotificationCreateDto dto);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
