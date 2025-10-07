using BookingApp.Models;

namespace BookingApp.Interface.IRepository
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(Booking booking);
        Task SaveAsync();
    }
}
