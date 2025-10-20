using BookingApp.DTOs.DoctorProfile;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    public class DoctorProfileController : ControllerBase
    {
        private readonly IDoctorProfileService _service;
        private readonly ILogger<DoctorProfileController> _logger;

        public DoctorProfileController(IDoctorProfileService service, ILogger<DoctorProfileController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
              [FromQuery] string? specialty = null,
              [FromQuery] int _start = 0,
              [FromQuery] int _end = 10,
              [FromQuery] string? _sort = "Id",
              [FromQuery] string? _order = "ASC")
        {
            var (doctors, totalCount) = await _service.GetAllAsync(
                _start,
                _end - _start,
                _sort ?? "Id",
                _order ?? "ASC",
                specialty
            );

            return Ok(doctors);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound(new { message = "Doctor not found." }) : Ok(result);
        }

        [HttpGet("{id}/details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorDetails(int id)
        {
            var result = await _service.GetDoctorWithDetailsAsync(id);
            return result == null ? NotFound(new { message = "Doctor not found." }) : Ok(result);
        }

        [HttpGet("{id}/available-schedules")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSchedules(int id)
        {
            try
            {
                var schedules = await _service.GetAvailableSchedulesAsync(id);
                return Ok(schedules);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ⭐ Endpoint mới: Tạo Doctor kèm User (dùng cho React Admin)
        [HttpPost("admin/create-with-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDoctorWithUser([FromBody] CreateDoctorWithUserDto dto)
        {
            try
            {
                var result = await _service.CreateDoctorWithUserAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] DoctorProfileCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorProfileUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "Doctor ID mismatch." });

            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Deleted doctor successfully." });
        }
    }
}
