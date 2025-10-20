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

        /// <summary>
        /// [Admin] Get all payments
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            return Ok(await _service.GetAllAsync());
        }

        /// <summary>
        /// [Admin/Doctor/Patient] Get payment by booking ID
        /// </summary>
        [HttpGet("booking/{bookingId}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetPaymentByBooking(int bookingId)
        {
            var result = await _service.GetByBookingIdAsync(bookingId);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// [Admin/Patient] Get all payments by patient ID
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> GetPaymentsByPatient(int patientId)
        {
            return Ok(await _service.GetByPatientAsync(patientId));
        }

        /// <summary>
        /// [Admin/Doctor] Create new payment
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetPaymentByBooking), new { bookingId = result.BookingId }, result);
        }

        /// <summary>
        /// [Admin] Update payment status
        /// </summary>
        [HttpPut("admin/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] PaymentUpdateDto dto)
        {
            var updated = await _service.UpdateStatusAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        /// <summary>
        /// [Admin] Delete payment
        /// </summary>
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        /// <summary>
        /// [Admin] Manually sync booking status with payment
        /// </summary>
        [HttpPost("admin/{id}/sync-booking")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SyncBookingStatus(int id)
        {
            var success = await _service.SyncBookingStatusAsync(id);
            return success ? Ok(new { message = "Booking synced successfully" }) : NotFound();
        }
    }
}