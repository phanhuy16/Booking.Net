using BookingApp.Data;
using BookingApp.Interface.IRepository;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.PatientProfile)
                .ThenInclude(p => p.User)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment?> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        public async Task<IEnumerable<Payment>> GetByPatientIdAsync(int patientId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .Where(p => p.Booking.PatientId == patientId)
                .ToListAsync();
        }

        public async Task AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Payment payment)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
