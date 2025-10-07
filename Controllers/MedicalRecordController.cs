using BookingApp.DTOs.MedicalRecord;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingApp.Controllers
{
    [Route("api/medical-records")]
    [ApiController]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordService _service;

        public MedicalRecordController(IMedicalRecordService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("my-records")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetByPatient()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _service.GetByPatientAsync(userId));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string role = User.FindFirstValue(ClaimTypes.Role)!;
            var record = await _service.GetByIdAsync(id, userId, role);
            return record == null ? NotFound() : Ok(record);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [RequestSizeLimit(20_000_000)] // giới hạn 20MB
        public async Task<IActionResult> Create([FromForm] MedicalRecordCreateDto dto, IFormFile? attachment)
        {
            int doctorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.CreateAsync(dto, doctorUserId, attachment);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
    }
}
