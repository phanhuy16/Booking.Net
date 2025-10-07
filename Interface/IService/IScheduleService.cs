using BookingApp.Common;
using BookingApp.DTOs.Schedule;

namespace BookingApp.Interface.IService
{
    public interface IScheduleService
    {
        Task<IEnumerable<ScheduleDto>> GetAllAsync();
        Task<PagedResult<ScheduleDto>> GetPagedAsync(ScheduleQueryParams query);
        Task<ScheduleDto?> GetByIdAsync(int id);
        Task<IEnumerable<ScheduleDto>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<ScheduleDto>> GetAvailableByDoctorAsync(int doctorId);
        Task<ScheduleDto> CreateAsync(ScheduleCreateDto dto);
        Task<bool> UpdateAsync(int id, ScheduleUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
