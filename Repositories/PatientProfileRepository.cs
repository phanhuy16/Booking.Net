using BookingApp.Data;
using BookingApp.Interface.IRepository;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Repositories
{
    public class PatientProfileRepository : IPatientProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<PatientProfile> GetAllAsync(bool includeDetails = false)
        {
            IQueryable<PatientProfile> query = _context.PatientProfiles
                .Include(p => p.User);

            if (includeDetails)
                query = query
                    .Include(p => p.Bookings).ThenInclude(b => b.DoctorProfile).ThenInclude(d => d.User)
                    .Include(p => p.Bookings).ThenInclude(b => b.Service)
                    .Include(p => p.MedicalRecords).AsSplitQuery();

            return query;
        }

        public async Task<PatientProfile?> GetByIdAsync(int id, bool includeDetails = false)
        {
            IQueryable<PatientProfile> query = _context.PatientProfiles
                .Include(p => p.User);

            if (includeDetails)
                query = query
                    .Include(p => p.Bookings).ThenInclude(b => b.DoctorProfile).ThenInclude(d => d.User)
                    .Include(p => p.Bookings).ThenInclude(b => b.Service)
                    .Include(p => p.MedicalRecords);

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(PatientProfile patient)
        {
            await _context.PatientProfiles.AddAsync(patient);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PatientProfile patient)
        {
            _context.PatientProfiles.Update(patient);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PatientProfile patient)
        {
            _context.PatientProfiles.Remove(patient);
            await _context.SaveChangesAsync();
        }
    }
}
