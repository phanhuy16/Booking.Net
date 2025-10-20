using BookingApp.DTOs.Specialty;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Route("api/specialties")]
    [ApiController]
    public class SpecialtyController : ControllerBase
    {
        private readonly ISpecialtyService _service;

        public SpecialtyController(ISpecialtyService service)
        {
            _service = service;
        }

        /// <summary>
        /// [Public] Get all specialties
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSpecialties() =>
            Ok(await _service.GetAllAsync());

        /// <summary>
        /// [Public] Get specialty by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSpecialtyById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// [Public] Get doctors by specialty with filters and pagination
        /// </summary>
        [HttpGet("{id}/doctors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorsBySpecialty(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "rating",
            [FromQuery] string order = "desc",
            [FromQuery] double? minRating = null,
            [FromQuery] int? minExperience = null)
        {
            var (doctors, totalPages) = await _service.GetDoctorsBySpecialtyIdAsync(
                id, page, pageSize, sortBy, order, minRating, minExperience);
            return Ok(new
            {
                currentPage = page,
                totalPages,
                pageSize,
                sortBy,
                order,
                filters = new { minRating, minExperience },
                items = doctors
            });
        }

        /// <summary>
        /// [Admin] Create new specialty
        /// </summary>
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSpecialty([FromForm] SpecialtyCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetSpecialtyById), new { id = result.Id }, result);
        }

        /// <summary>
        /// [Admin] Update specialty
        /// </summary>
        [HttpPut("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSpecialty(int id, [FromForm] SpecialtyUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// [Admin] Delete specialty
        /// </summary>
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                return deleted ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}