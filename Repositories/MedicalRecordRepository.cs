using BookingApp.Data;
using BookingApp.Interface.IRepository;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Repositories
{
    public class MedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicalRecord>> GetAllAsync()
        {
            return await _context.MedicalRecords
                .Include(m => m.DoctorProfile).ThenInclude(d => d.User)
                .Include(m => m.PatientProfile).ThenInclude(p => p.User)
                .OrderByDescending(m => m.RecordedAt)
                .ToListAsync();
        }

        public async Task<MedicalRecord?> GetByIdAsync(int id)
        {
            return await _context.MedicalRecords
                .Include(m => m.DoctorProfile).ThenInclude(d => d.User)
                .Include(m => m.PatientProfile).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId)
        {
            return await _context.MedicalRecords
                .Include(m => m.DoctorProfile).ThenInclude(d => d.User)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.RecordedAt)
                .ToListAsync();
        }

        public async Task AddAsync(MedicalRecord record)
        {
            await _context.MedicalRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

    }
}
