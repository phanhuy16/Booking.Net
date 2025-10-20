using AutoMapper;
using BookingApp.DTOs.Service;
using BookingApp.Interface.IService;
using BookingApp.Models;
using BookingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Route("api/services")]
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

        /// <summary>
        /// [Public] Get all services
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllServices()
        {
            var result = await _serviceManager.GetAllServicesAsync();
            return Ok(result);
        }

        /// <summary>
        /// [Admin/Doctor/Patient] Get service by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var result = await _serviceManager.GetServiceByIdAsync(id);
            return result == null ? NotFound(new { message = "Service not found." }) : Ok(result);
        }

        /// <summary>
        /// [Admin] Create new service
        /// </summary>
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateService([FromBody] ServiceCreateDto dto)
        {
            try
            {
                var result = await _serviceManager.AddServiceAsync(dto);
                return CreatedAtAction(nameof(GetServiceById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate service creation attempt.");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// [Admin] Update service
        /// </summary>
        [HttpPut("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceUpdateDto dto)
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

        /// <summary>
        /// [Admin] Delete service
        /// </summary>
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
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