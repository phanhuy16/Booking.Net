using BookingApp.DTOs.PatientProfile;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Route("api/patients")]
    [ApiController]
    public class PatientProfileController : ControllerBase
    {
        private readonly IPatientProfileService _service;

        public PatientProfileController(IPatientProfileService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("{id}/summary")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetDashboard(int id)
        {
            var dashboard = await _service.GetDashboardAsync(id);
            return dashboard == null ? NotFound(new { message = "Patient not found." }) : Ok(dashboard);
        }

        [HttpGet("{id}/statistics")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetStatistics(int id, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from == default || to == default)
                return BadRequest(new { message = "Please provide valid 'from' and 'to' query parameters." });

            var stats = await _service.GetStatisticsAsync(id, from, to);
            return stats == null ? NotFound(new { message = "Patient not found." }) : Ok(stats);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] PatientProfileCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientProfileUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated ? Ok(new { message = "Updated successfully" }) : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? Ok(new { message = "Deleted successfully" }) : NotFound();
        }
    }
}
