using AutoMapper;
using BookingApp.Common;
using BookingApp.DTOs.Schedule;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;

namespace BookingApp.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(IScheduleRepository repo, IMapper mapper, ILogger<ScheduleService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ScheduleDto>> GetAllAsync()
        {
            var schedules = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }

        public async Task<PagedResult<ScheduleDto>> GetPagedAsync(ScheduleQueryParams query)
        {
            var (items, totalCount) = await _repo.GetPagedAsync(query);
            var mapped = _mapper.Map<IEnumerable<ScheduleDto>>(items);

            return new PagedResult<ScheduleDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<ScheduleDto?> GetByIdAsync(int id)
        {
            var schedule = await _repo.GetByIdAsync(id);
            return schedule == null ? null : _mapper.Map<ScheduleDto>(schedule);
        }

        public async Task<IEnumerable<ScheduleDto>> GetByDoctorIdAsync(int doctorId)
        {
            var schedules = await _repo.GetByDoctorIdAsync(doctorId);
            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }

        public async Task<IEnumerable<ScheduleDto>> GetAvailableByDoctorAsync(int doctorId)
        {
            var schedules = await _repo.GetAvailableByDoctorAsync(doctorId);
            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }

        public async Task<ScheduleDto> CreateAsync(ScheduleCreateDto dto)
        {
            var schedule = _mapper.Map<Schedule>(dto);
            await _repo.AddAsync(schedule);
            return _mapper.Map<ScheduleDto>(schedule);
        }

        public async Task<bool> UpdateAsync(int id, ScheduleUpdateDto dto)
        {
            var schedule = await _repo.GetByIdAsync(id);
            if (schedule == null) return false;

            _mapper.Map(dto, schedule);
            await _repo.UpdateAsync(schedule);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var schedule = await _repo.GetByIdAsync(id);
            if (schedule == null || schedule.Bookings.Any()) return false;

            await _repo.DeleteAsync(schedule);
            return true;
        }
    }
}
