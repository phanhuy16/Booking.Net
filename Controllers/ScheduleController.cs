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

        // GET: api/schedules
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetPaged([FromQuery] ScheduleQueryParams query)
        {
            var result = await _service.GetPagedAsync(query);
            return Ok(result);
        }

        // GET: api/schedules/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        // GET: api/schedules/doctor/{doctorId}
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            return Ok(await _service.GetByDoctorIdAsync(doctorId));
        }

        // GET: api/schedules/doctor/{doctorId}/available
        [HttpGet("doctor/{doctorId}/available")]
        [AllowAnonymous] // Bệnh nhân cần xem để đặt lịch
        public async Task<IActionResult> GetAvailable(int doctorId)
        {
            return Ok(await _service.GetAvailableByDoctorAsync(doctorId));
        }

        // POST: api/schedules
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create([FromBody] ScheduleCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT: api/schedules/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Update(int id, [FromBody] ScheduleUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        // DELETE: api/schedules/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : BadRequest(new { message = "Cannot delete schedule with active bookings." });
        }
    }
}
