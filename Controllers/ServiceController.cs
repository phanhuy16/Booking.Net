using AutoMapper;
using BookingApp.DTOs.Service;
using BookingApp.Interface.IService;
using BookingApp.Models;
using BookingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(IServiceManager serviceManager, ILogger<ServiceController> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _serviceManager.GetAllServicesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _serviceManager.GetServiceByIdAsync(id);
            return result == null ? NotFound(new { message = "Service not found." }) : Ok(result);
        }

        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ServiceCreateDto dto)
        {
            try
            {
                var result = await _serviceManager.AddServiceAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate service creation attempt.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "Service ID mismatch." });

            try
            {
                var result = await _serviceManager.UpdateServiceAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _serviceManager.DeleteServiceAsync(id);
                return Ok(new { message = "Deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
