using BookingApp.DTOs.Schedule;
using BookingApp.Models;

namespace BookingApp.Interface.IRepository
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<Schedule>> GetAllAsync();
        Task<(IEnumerable<Schedule> items, int totalCount)> GetPagedAsync(ScheduleQueryParams query);
        Task<Schedule?> GetByIdAsync(int id);
        Task<IEnumerable<Schedule>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<Schedule>> GetAvailableByDoctorAsync(int doctorId);
        Task AddAsync(Schedule schedule);
        Task UpdateAsync(Schedule schedule);
        Task DeleteAsync(Schedule schedule);
    }
}
