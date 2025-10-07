using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.DoctorProfile;
using BookingApp.DTOs.Specialty;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly ISpecialtyRepository _repo;
        private readonly IMapper _mapper;
        private readonly FirebaseStorageService _firebase;
        private readonly ILogger<SpecialtyService> _logger;
        private readonly ApplicationDbContext _context;

        public SpecialtyService(
            ISpecialtyRepository repo,
            IMapper mapper,
            FirebaseStorageService firebase,
            ILogger<SpecialtyService> logger,
            ApplicationDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _firebase = firebase;
            _logger = logger;
            _context = context;
        }

        public async Task<IEnumerable<SpecialtyDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<SpecialtyDto>>(items);
        }

        public async Task<SpecialtyDto?> GetByIdAsync(int id)
        {
            var specialty = await _repo.GetByIdAsync(id);
            return specialty == null ? null : _mapper.Map<SpecialtyDto>(specialty);
        }

        // 🧠 Hàm nâng cấp: có lọc, sắp xếp và phân trang
        public async Task<(IEnumerable<DoctorInSpecialtyDto> Doctors, int TotalPages)>
            GetDoctorsBySpecialtyIdAsync(
                int specialtyId,
                int page,
                int pageSize,
                string sortBy,
                string order,
                double? minRating,
                int? minExperience)
        {
            var query = _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Feedbacks)
                .Where(d => d.SpecialtyId == specialtyId)
                .AsQueryable();

            // 🩺 Lọc nâng cao
            if (minExperience.HasValue)
                query = query.Where(d => d.ExperienceYears >= minExperience.Value);

            if (minRating.HasValue)
                query = query.Where(d => d.Feedbacks.Any()
                    && d.Feedbacks.Average(f => f.Rating) >= minRating.Value);

            // 🧭 Sắp xếp
            switch (sortBy.ToLower())
            {
                case "experience":
                    query = (order == "desc")
                        ? query.OrderByDescending(d => d.ExperienceYears)
                        : query.OrderBy(d => d.ExperienceYears);
                    break;
                case "name":
                    query = (order == "desc")
                        ? query.OrderByDescending(d => d.User.FullName)
                        : query.OrderBy(d => d.User.FullName);
                    break;
                default: // rating
                    query = (order == "asc")
                        ? query.OrderBy(d => d.Feedbacks.Any() ? d.Feedbacks.Average(f => f.Rating) : 0)
                        : query.OrderByDescending(d => d.Feedbacks.Any() ? d.Feedbacks.Average(f => f.Rating) : 0);
                    break;
            }

            // 📄 Phân trang
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var doctors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = doctors.Select(d => new DoctorInSpecialtyDto
            {
                Id = d.Id,
                FullName = d.User.FullName,
                Workplace = d.Workplace,
                ExperienceYears = d.ExperienceYears,
                AverageRating = d.Feedbacks.Any() ? d.Feedbacks.Average(f => f.Rating) : 0,
                TotalFeedbacks = d.Feedbacks.Count
            }).ToList();

            return (result, totalPages);
        }

        public async Task<SpecialtyDto> CreateAsync(SpecialtyCreateDto dto)
        {
            string? iconUrl = null;
            if (dto.Icon != null)
                iconUrl = await _firebase.UploadFileAsync(dto.Icon, "specialties");

            var specialty = new Specialty
            {
                Name = dto.Name,
                Description = dto.Description,
                IconUrl = iconUrl
            };

            await _repo.AddAsync(specialty);
            _logger.LogInformation("Created specialty: {Name}", specialty.Name);

            return _mapper.Map<SpecialtyDto>(specialty);
        }

        public async Task<SpecialtyDto?> UpdateAsync(int id, SpecialtyUpdateDto dto)
        {
            var specialty = await _repo.GetByIdAsync(id);
            if (specialty == null) return null;

            specialty.Name = dto.Name;
            specialty.Description = dto.Description;

            if (dto.Icon != null)
            {
                if (!string.IsNullOrEmpty(specialty.IconUrl))
                    await _firebase.DeleteFileAsync(specialty.IconUrl);

                specialty.IconUrl = await _firebase.UploadFileAsync(dto.Icon, "specialties");
            }

            await _repo.UpdateAsync(specialty);
            return _mapper.Map<SpecialtyDto>(specialty);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var specialty = await _repo.GetByIdAsync(id);
            if (specialty == null) return false;

            if (specialty.DoctorProfiles.Any())
                throw new InvalidOperationException("Cannot delete specialty with existing doctors.");

            if (!string.IsNullOrEmpty(specialty.IconUrl))
                await _firebase.DeleteFileAsync(specialty.IconUrl);

            await _repo.DeleteAsync(specialty);
            return true;
        }
    }
}
