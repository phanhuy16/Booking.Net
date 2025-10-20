using BookingApp.DTOs.Notification;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingApp.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        /// <summary>
        /// [All authenticated users] Get my notifications
        /// </summary>
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var notifications = await _service.GetAllByUserIdAsync(userId);
            return Ok(notifications);
        }

        /// <summary>
        /// [All authenticated users] Get notification by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            var notif = await _service.GetByIdAsync(id);
            return notif == null ? NotFound() : Ok(notif);
        }

        /// <summary>
        /// [Admin] Create notification manually
        /// </summary>
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetNotificationById), new { id = result.Id }, result);
        }

        /// <summary>
        /// [All authenticated users] Mark notification as read
        /// </summary>
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var result = await _service.MarkAsReadAsync(id);
            return result ? Ok(new { message = "Marked as read" }) : NotFound();
        }

        /// <summary>
        /// [All authenticated users] Delete notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}