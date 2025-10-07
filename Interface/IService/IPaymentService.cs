using BookingApp.DTOs.Payment;

namespace BookingApp.Interface.IService
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllAsync();
        Task<PaymentDto?> GetByIdAsync(int id);
        Task<PaymentDto?> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<PaymentDto>> GetByPatientAsync(int patientId);
        Task<PaymentDto> CreateAsync(PaymentCreateDto dto);
        Task<bool> UpdateStatusAsync(int id, PaymentUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        // ✅ Đồng bộ Booking khi Payment thay đổi
        Task<bool> SyncBookingStatusAsync(int paymentId);
    }
}
