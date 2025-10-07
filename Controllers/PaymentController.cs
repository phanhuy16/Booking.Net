using BookingApp.DTOs.Payment;
using BookingApp.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }

        // GET: api/payments
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET: api/payments/booking/{bookingId}
        [HttpGet("booking/{bookingId}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetByBooking(int bookingId)
        {
            var result = await _service.GetByBookingIdAsync(bookingId);
            return result == null ? NotFound() : Ok(result);
        }

        // GET: api/payments/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            return Ok(await _service.GetByPatientAsync(patientId));
        }

        // POST: api/payments
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByBooking), new { bookingId = result.BookingId }, result);
        }

        // PUT: api/payments/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] PaymentUpdateDto dto)
        {
            var updated = await _service.UpdateStatusAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        // DELETE: api/payments/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        // ✅ API tùy chọn: ép sync thủ công
        [HttpPost("sync-booking/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SyncBooking(int id)
        {
            var success = await _service.SyncBookingStatusAsync(id);
            return success ? Ok(new { message = "Booking synced successfully" }) : NotFound();
        }
    }
}
