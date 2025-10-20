using BookingApp.Data;
using BookingApp.Interface.IRepository;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Repositories
{
    public class DoctorProfileRepository : IDoctorProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<DoctorProfile> GetAllAsync(bool includeDetails = false)
        {
            IQueryable<DoctorProfile> query = _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Specialty);

            if (includeDetails)
                query = query.Include(d => d.Schedules).Include(d => d.Feedbacks);

            return query; // Bỏ await và ToListAsync()
        }

        public async Task<DoctorProfile?> GetByIdAsync(int id, bool includeDetails = false)
        {
            IQueryable<DoctorProfile> query = _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Specialty);

            if (includeDetails)
                query = query.Include(d => d.Schedules).Include(d => d.Feedbacks);

            return await query.FirstOrDefaultAsync(d => d.Id == id);
        }


        public async Task AddAsync(DoctorProfile doctor)
        {
            await _context.DoctorProfiles.AddAsync(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DoctorProfile doctor)
        {
            _context.DoctorProfiles.Update(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(DoctorProfile doctor)
        {
            _context.DoctorProfiles.Remove(doctor);
            await _context.SaveChangesAsync();
        }
    }
}
