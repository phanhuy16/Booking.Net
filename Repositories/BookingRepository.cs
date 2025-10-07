using BookingApp.Data;
using BookingApp.Interface.IRepository;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.PatientProfile)
                .Include(b => b.DoctorProfile)
                .Include(b => b.Schedule)
                .ToListAsync();
        }
        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Schedule)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }
        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await Task.CompletedTask;
        }
        public async Task DeleteAsync(Booking booking)
        {
            _context.Bookings.Remove(booking);
            await Task.CompletedTask;
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
