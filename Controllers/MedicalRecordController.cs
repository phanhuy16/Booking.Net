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

        /// <summary>
        /// [Admin] Get all medical records
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMedicalRecords() =>
            Ok(await _service.GetAllAsync());

        /// <summary>
        /// [Patient] Get my medical records
        /// </summary>
        [HttpGet("patient/my-records")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyMedicalRecords()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _service.GetByPatientAsync(userId));
        }

        /// <summary>
        /// [Patient/Doctor/Admin] Get medical record by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetMedicalRecordById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string role = User.FindFirstValue(ClaimTypes.Role)!;
            var record = await _service.GetByIdAsync(id, userId, role);
            return record == null ? NotFound() : Ok(record);
        }

        /// <summary>
        /// [Doctor] Create new medical record (max 20MB attachment)
        /// </summary>
        [HttpPost("doctor")]
        [Authorize(Roles = "Doctor")]
        [RequestSizeLimit(20_000_000)] // 20MB limit
        public async Task<IActionResult> CreateMedicalRecord([FromForm] MedicalRecordCreateDto dto, IFormFile? attachment)
        {
            int doctorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.CreateAsync(dto, doctorUserId, attachment);
            return CreatedAtAction(nameof(GetMedicalRecordById), new { id = result.Id }, result);
        }
    }
}