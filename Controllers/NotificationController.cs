using BookingApp.DTOs.Notification;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingApp.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        // 📩 Lấy danh sách thông báo của user hiện tại
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var notifications = await _service.GetAllByUserIdAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var notif = await _service.GetByIdAsync(id);
            return notif == null ? NotFound() : Ok(notif);
        }

        // 🆕 Admin có thể tạo thông báo thủ công
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // ✅ Đánh dấu đã đọc
        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _service.MarkAsReadAsync(id);
            return result ? Ok(new { message = "Marked as read" }) : NotFound();
        }

        // ❌ Xoá thông báo
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
