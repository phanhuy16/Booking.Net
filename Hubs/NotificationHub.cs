using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BookingApp.Hubs
{
    // Client phải authenticated (JWT) mới kết nối được
    [Authorize]
    public class NotificationHub : Hub
    {
        // Optional: override OnConnectedAsync để log hoặc gửi welcome
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // maps to claim nameidentifier
            // bạn có thể group người dùng theo userId hoặc role nếu muốn
            await base.OnConnectedAsync();
        }

        // Client có thể gọi để đánh dấu đã đọc (tùy chọn)
        public async Task MarkNotificationAsRead(int notificationId)
        {
            // Nếu muốn, bạn có thể kiểm tra ownership ở đây hoặc ở service
            // Implementation can call NotificationService to mark read.
            await Clients.Caller.SendAsync("NotificationMarked", notificationId);
        }
    }
}
