using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.DoctorProfile;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Services
{
    public class DoctorProfileService : IDoctorProfileService
    {
        private readonly IDoctorProfileRepository _repo;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorProfileService> _logger;

        public DoctorProfileService(IDoctorProfileRepository repo, ApplicationDbContext context, IMapper mapper, ILogger<DoctorProfileService> logger)
        {
            _repo = repo;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DoctorProfileDto>> GetAllAsync(string? specialty = null)
        {
            var doctors = await _repo.GetAllAsync();
            if (!string.IsNullOrEmpty(specialty))
            {
                doctors = doctors.Where(d => d.Specialty.Name.ToLower().Contains(specialty.ToLower()));
            }
            return _mapper.Map<IEnumerable<DoctorProfileDto>>(doctors);
        }

        public async Task<DoctorProfileDto?> GetByIdAsync(int id)
        {
            var doctor = await _repo.GetByIdAsync(id);
            return doctor == null ? null : _mapper.Map<DoctorProfileDto>(doctor);
        }

        public async Task<DoctorProfileWithDetailsDto?> GetDoctorWithDetailsAsync(int id)
        {
            var doctor = await _repo.GetByIdAsync(id, includeDetails: true);
            return doctor == null ? null : _mapper.Map<DoctorProfileWithDetailsDto>(doctor);
        }

        public async Task<IEnumerable<DoctorScheduleDto>> GetAvailableSchedulesAsync(int doctorId)
        {
            var doctor = await _repo.GetByIdAsync(doctorId, includeDetails: true);
            if (doctor == null)
                throw new KeyNotFoundException("Doctor not found.");

            return doctor.Schedules
                .Where(s => s.IsAvailable)
                .Select(s => new DoctorScheduleDto
                {
                    ScheduleId = s.Id,
                    Date = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList();
        }

        public async Task<DoctorProfileDto> CreateAsync(DoctorProfileCreateDto dto)
        {
            // Kiểm tra user có tồn tại
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var specialty = await _context.Specialties.FindAsync(dto.SpecialtyId);
            if (specialty == null)
                throw new InvalidOperationException("Specialty not found.");

            var entity = _mapper.Map<DoctorProfile>(dto);
            await _repo.AddAsync(entity);

            _logger.LogInformation("Created new doctor profile for user {UserId}", dto.UserId);
            return _mapper.Map<DoctorProfileDto>(entity);
        }

        public async Task<DoctorProfileDto> UpdateAsync(int id, DoctorProfileUpdateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Doctor profile not found.");

            _mapper.Map(dto, existing);
            await _repo.UpdateAsync(existing);

            _logger.LogInformation("Updated doctor profile {Id}", id);
            return _mapper.Map<DoctorProfileDto>(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Doctor profile not found.");

            // Kiểm tra có booking đang hoạt động
            bool hasActiveBookings = await _context.Bookings.AnyAsync(b => b.DoctorId == id && b.Status != BookingStatus.Cancelled);
            if (hasActiveBookings)
                throw new InvalidOperationException("Cannot delete doctor with active bookings.");

            await _repo.DeleteAsync(existing);
            _logger.LogInformation("Deleted doctor profile {Id}", id);
        }
    }
}
