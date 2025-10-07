using BookingApp.DTOs.Feedback;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingApp.Controllers
{
    [Route("api/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IFeedbackService service, ILogger<FeedbackController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("doctor/{doctorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByDoctorId(
       int doctorId,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 5)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (feedbacks, totalPages) = await _service.GetByDoctorIdAsync(doctorId, page, pageSize, userId);

            return Ok(new
            {
                currentPage = page,
                totalPages,
                pageSize,
                items = feedbacks
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Update(int id, [FromBody] FeedbackUpdateDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.UpdateAsync(id, dto, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Patient,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                bool isAdmin = User.IsInRole("Admin");

                await _service.DeleteAsync(id, userId, isAdmin);
                return Ok(new { message = "Feedback deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create([FromBody] FeedbackCreateDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetByDoctorId), new { doctorId = result.DoctorId }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid feedback creation");
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
