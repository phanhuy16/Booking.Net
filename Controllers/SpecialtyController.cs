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

        [HttpGet("get-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

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

        [HttpPost("admin/add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] SpecialtyCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("admin/update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] SpecialtyUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("admin/delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
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
