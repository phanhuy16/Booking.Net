using BookingApp.Data;
using BookingApp.Interface.IRepository;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Repositories
{
    public class SpecialtyRepository : ISpecialtyRepository
    {
        private readonly ApplicationDbContext _context;

        public SpecialtyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Specialty>> GetAllAsync() =>
            await _context.Specialties.Include(s => s.DoctorProfiles).ToListAsync();

        public async Task<Specialty?> GetByIdAsync(int id) =>
            await _context.Specialties.Include(s => s.DoctorProfiles).FirstOrDefaultAsync(s => s.Id == id);

        public async Task AddAsync(Specialty specialty)
        {
            _context.Specialties.Add(specialty);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Specialty specialty)
        {
            _context.Specialties.Update(specialty);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Specialty specialty)
        {
            _context.Specialties.Remove(specialty);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
