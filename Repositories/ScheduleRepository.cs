using BookingApp.Data;
using BookingApp.DTOs.Schedule;
using BookingApp.Interface.IRepository;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public ScheduleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Schedule>> GetAllAsync()
        {
            return await _context.Schedules
                .Include(s => s.DoctorProfile)
                .ThenInclude(d => d.User)
                .Include(s => s.Bookings)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Schedule> items, int totalCount)> GetPagedAsync(ScheduleQueryParams query)
        {
            var schedules = _context.Schedules
                .Include(s => s.DoctorProfile)
                .ThenInclude(d => d.User)
                .Include(s => s.Bookings)
                .AsQueryable();

            // 📅 Filtering
            if (query.FromDate.HasValue)
                schedules = schedules.Where(s => s.Date >= query.FromDate.Value.Date);
            if (query.ToDate.HasValue)
                schedules = schedules.Where(s => s.Date <= query.ToDate.Value.Date);

            var totalCount = await schedules.CountAsync();

            // 📖 Pagination
            var items = await schedules
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Schedule?> GetByIdAsync(int id)
        {
            return await _context.Schedules
                .Include(s => s.DoctorProfile)
                .ThenInclude(d => d.User)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Schedule>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.Schedules
                .Include(s => s.Bookings)
                .Where(s => s.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetAvailableByDoctorAsync(int doctorId)
        {
            return await _context.Schedules
                .Where(s => s.DoctorId == doctorId && s.IsAvailable)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task AddAsync(Schedule schedule)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Schedule schedule)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
