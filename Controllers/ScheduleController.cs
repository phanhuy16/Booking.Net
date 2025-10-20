using BookingApp.DTOs.Schedule;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Route("api/schedules")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _service;

        public ScheduleController(IScheduleService service)
        {
            _service = service;
        }

        /// <summary>
        /// [Admin/Doctor] Get all schedules
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAllSchedules()
        {
            return Ok(await _service.GetAllAsync());
        }

        /// <summary>
        /// [Admin/Doctor] Get schedules with pagination
        /// </summary>
        [HttpGet("admin/paged")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetPagedSchedules([FromQuery] ScheduleQueryParams query)
        {
            var result = await _service.GetPagedAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// [Admin/Doctor/Patient] Get schedule by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// [Admin/Doctor/Patient] Get all schedules by doctor ID
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetSchedulesByDoctor(int doctorId)
        {
            return Ok(await _service.GetByDoctorIdAsync(doctorId));
        }

        /// <summary>
        /// [Public] Get available schedules by doctor ID
        /// </summary>
        [HttpGet("doctor/{doctorId}/available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSchedulesByDoctor(int doctorId)
        {
            return Ok(await _service.GetAvailableByDoctorAsync(doctorId));
        }

        /// <summary>
        /// [Admin/Doctor] Create new schedule
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetScheduleById), new { id = result.Id }, result);
        }

        /// <summary>
        /// [Admin/Doctor] Update schedule
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] ScheduleUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        /// <summary>
        /// [Admin/Doctor] Delete schedule
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : BadRequest(new { message = "Cannot delete schedule with active bookings." });
        }
    }
}