using AutoMapper;
using BookingApp.DTOs.Notification;
using BookingApp.Hubs;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.AspNetCore.SignalR;

namespace BookingApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(
            INotificationRepository repo,
            IMapper mapper,
            IHubContext<NotificationHub> hub)
        {
            _repo = repo;
            _mapper = mapper;
            _hub = hub;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllByUserIdAsync(int userId)
        {
            var notifs = await _repo.GetAllByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationDto>>(notifs);
        }

        public async Task<NotificationDto?> GetByIdAsync(int id)
        {
            var notif = await _repo.GetByIdAsync(id);
            return notif == null ? null : _mapper.Map<NotificationDto>(notif);
        }

        public async Task<NotificationDto> CreateAsync(NotificationCreateDto dto)
        {
            var notif = _mapper.Map<Notification>(dto);
            notif.CreatedAt = DateTime.UtcNow;

            await _repo.AddAsync(notif);
            var dtoOut = _mapper.Map<NotificationDto>(notif);

            // Gửi real-time tới user (dùng Claim nameidentifier => userId string)
            // Clients.User expects string user identifier (usually userId)
            await _hub.Clients.User(notif.UserId.ToString())
                      .SendAsync("ReceiveNotification", dtoOut);

            return dtoOut;
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var notif = await _repo.GetByIdAsync(id);
            if (notif == null) return false;

            notif.IsRead = true;
            await _repo.UpdateAsync(notif);

            // Notify client (caller) that this was marked
            await _hub.Clients.User(notif.UserId.ToString()).SendAsync("NotificationUpdated", id, true);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notif = await _repo.GetByIdAsync(id);
            if (notif == null) return false;

            await _repo.DeleteAsync(notif);
            await _hub.Clients.User(notif.UserId.ToString()).SendAsync("NotificationDeleted", id);
            return true;
        }
    }
}
