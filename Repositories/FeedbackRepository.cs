using BookingApp.Data;
using BookingApp.Interface.IRepository;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public FeedbackRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Feedback>> GetByDoctorIdAsync(int doctorId, int page, int pageSize)
        {
            return await _context.Feedbacks
                .Include(f => f.DoctorProfile).ThenInclude(d => d.User)
                .Include(f => f.PatientProfile).ThenInclude(p => p.User)
                .Where(f => f.DoctorId == doctorId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByDoctorIdAsync(int doctorId)
        {
            return await _context.Feedbacks.CountAsync(f => f.DoctorId == doctorId);
        }

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _context.Feedbacks
                .Include(f => f.DoctorProfile)
                .Include(f => f.PatientProfile)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task UpdateAsync(Feedback feedback)
        {
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Feedback feedback)
        {
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(Feedback feedback)
        {
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }
    }
}
