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

        /// <summary>
        /// [Admin/Doctor] Get all patients with pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAllPatients(
             [FromQuery] int _start = 0,
             [FromQuery] int _end = 10,
             [FromQuery] string? _sort = "Id",
             [FromQuery] string? _order = "ASC")
        {
            var (patients, totalCount) = await _service.GetAllAsync(
                _start,
                _end - _start,
                _sort ?? "Id",
                _order ?? "ASC"
            );
            var patientsList = patients.ToList();
            return Ok(patientsList);
        }

        /// <summary>
        /// [Admin/Doctor/Patient] Get patient profile by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// [Admin/Doctor/Patient] Get patient dashboard summary
        /// </summary>
        [HttpGet("{id}/summary")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetPatientDashboard(int id)
        {
            var dashboard = await _service.GetDashboardAsync(id);
            return dashboard == null ? NotFound(new { message = "Patient not found." }) : Ok(dashboard);
        }

        /// <summary>
        /// [Admin/Doctor/Patient] Get patient statistics by date range
        /// </summary>
        [HttpGet("{id}/statistics")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetPatientStatistics(int id, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from == default || to == default)
                return BadRequest(new { message = "Please provide valid 'from' and 'to' query parameters." });
            var stats = await _service.GetStatisticsAsync(id, from, to);
            return stats == null ? NotFound(new { message = "Patient not found." }) : Ok(stats);
        }

        /// <summary>
        /// [Admin] Create new patient profile
        /// </summary>
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePatient([FromBody] PatientProfileCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetPatientById), new { id = result.Id }, result);
        }

        /// <summary>
        /// [Admin/Patient] Update patient profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientProfileUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated ? Ok(new { message = "Updated successfully" }) : NotFound();
        }

        /// <summary>
        /// [Admin] Delete patient profile
        /// </summary>
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? Ok(new { message = "Deleted successfully" }) : NotFound();
        }
    }
}