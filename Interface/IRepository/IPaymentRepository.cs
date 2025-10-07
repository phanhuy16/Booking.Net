using BookingApp.Models;

namespace BookingApp.Interface.IRepository
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<Payment>> GetByPatientIdAsync(int patientId);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(Payment payment);
        Task SaveAsync();
    }
}
