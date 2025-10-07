using BookingApp.DTOs.Booking;

namespace BookingApp.Interface.IService
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDto>> GetAllAsync();
        Task<BookingDto?> GetByIdAsync(int id);
        Task<BookingDto> CreateAsync(BookingCreateDto dto);
        Task<bool> UpdateStatusAsync(int id, BookingUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
