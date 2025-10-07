using BookingApp.DTOs.Booking;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả các lịch đặt khám.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _bookingService.GetAllAsync();
            return Ok(bookings);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một lịch đặt khám theo ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
                return NotFound(new { message = "Booking not found." });

            return Ok(booking);
        }

        /// <summary>
        /// Tạo mới một lịch đặt khám (bệnh nhân thực hiện).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create([FromBody] BookingCreateDto dto)
        {
            try
            {
                var result = await _bookingService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Create booking failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đặt lịch (bác sĩ hoặc admin thực hiện).
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] BookingUpdateDto dto)
        {
            try
            {
                var result = await _bookingService.UpdateStatusAsync(id, dto);
                return result ? Ok(new { message = "Booking status updated successfully." })
                              : NotFound(new { message = "Booking not found." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Update booking status failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Hủy lịch đặt khám (bệnh nhân hoặc admin).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _bookingService.DeleteAsync(id);
                return result ? Ok(new { message = "Booking deleted successfully." })
                              : NotFound(new { message = "Booking not found." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Delete booking failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
