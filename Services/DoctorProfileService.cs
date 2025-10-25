using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.DoctorProfile;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Services
{
    public class DoctorProfileService : IDoctorProfileService
    {
        private readonly IDoctorProfileRepository _repo;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorProfileService> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly FirebaseStorageService _firebase;

        public DoctorProfileService(
            IDoctorProfileRepository repo,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<DoctorProfileService> logger,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            FirebaseStorageService firebase)
        {
            _repo = repo;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _firebase = firebase;
        }

        public async Task<(IEnumerable<DoctorProfileDto> DoctorProfiles, int TotalCount)> GetAllAsync(
         int skip,
         int take,
         string sortBy,
         string sortOrder,
         string? specialty = null)
        {
            var query = _repo.GetAllAsync(includeDetails: true);

            // Filter theo specialty
            if (!string.IsNullOrEmpty(specialty))
            {
                query = query.Where(d => d.Specialty.Name.ToLower().Contains(specialty.ToLower()));
            }

            // Đếm tổng số
            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = (sortBy.ToLower(), sortOrder.ToUpper()) switch
                {
                    ("id", "DESC") => query.OrderByDescending(d => d.Id),
                    ("id", _) => query.OrderBy(d => d.Id),
                    _ => query.OrderBy(d => d.Id)
                };
            }

            // Pagination
            var doctors = await query.Skip(skip).Take(take).ToListAsync();
            var mapped = _mapper.Map<IEnumerable<DoctorProfileDto>>(doctors);

            return (mapped, totalCount);
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

            // Upload avatar nếu có
            if (dto.Avatar != null)
            {
                entity.AvatarUrl = await _firebase.UploadFileAsync(dto.Avatar, "doctors");
            }

            await _repo.AddAsync(entity);

            _logger.LogInformation("Created new doctor profile for user {UserId}", dto.UserId);
            return _mapper.Map<DoctorProfileDto>(entity);
        }

        // ⭐ Method mới: Tạo User + DoctorProfile
        public async Task<DoctorProfileDto> CreateDoctorWithUserAsync(CreateDoctorWithUserDto dto)
        {
            // 1. Kiểm tra specialty tồn tại
            var specialty = await _context.Specialties.FindAsync(dto.SpecialtyId);
            if (specialty == null)
                throw new InvalidOperationException("Specialty not found.");

            // 2. Kiểm tra email đã tồn tại
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already exists.");

            // 3. Tạo User mới với Identity
            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.Phone,
                DateJoined = DateTime.UtcNow,
                EmailConfirmed = true // Admin tạo thì confirmed luôn
            };

            var password = string.IsNullOrEmpty(dto.Password) ? "Doctor@123" : dto.Password;
            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            // 4. Assign role "Doctor"
            if (!await _roleManager.RoleExistsAsync("Doctor"))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>("Doctor"));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Doctor");
            if (!roleResult.Succeeded)
            {
                // Rollback: xóa user nếu assign role thất bại
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign role: {errors}");
            }

            // 5. Upload avatar nếu có
            string? avatarUrl = null;
            if (dto.Avatar != null)
            {
                try
                {
                    avatarUrl = await _firebase.UploadFileAsync(dto.Avatar, "doctors");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to upload avatar, continuing without avatar");
                }
            }

            // 6. Tạo DoctorProfile
            var profile = new DoctorProfile
            {
                UserId = user.Id,
                SpecialtyId = dto.SpecialtyId,
                ExperienceYears = dto.ExperienceYears,
                Description = dto.Description,
                Workplace = dto.Workplace,
                AvatarUrl = avatarUrl,
                ConsultationFee = dto.ConsultationFee,
                AverageRating = 0,
                TotalFeedbacks = 0
            };

            try
            {
                await _repo.AddAsync(profile);
                _logger.LogInformation("Created new doctor with user {Email}", dto.Email);
                return _mapper.Map<DoctorProfileDto>(profile);
            }
            catch (Exception ex)
            {
                // Rollback: xóa user nếu tạo profile thất bại
                await _userManager.DeleteAsync(user);
                if (avatarUrl != null)
                {
                    await _firebase.DeleteFileAsync(avatarUrl);
                }
                _logger.LogError(ex, "Failed to create doctor profile, rolled back user creation");
                throw new InvalidOperationException("Failed to create doctor profile", ex);
            }
        }

        public async Task<DoctorProfileDto> UpdateAsync(int id, DoctorProfileUpdateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Doctor profile not found.");

            // Update user info
            var user = await _userManager.FindByIdAsync(existing.UserId.ToString());
            if (user != null)
            {
                user.FullName = dto.FullName;
                user.Email = dto.Email;
                user.PhoneNumber = dto.Phone;

                // Update password nếu có
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, dto.Password);
                }

                await _userManager.UpdateAsync(user);
            }

            // Update avatar nếu có upload mới
            if (dto.Avatar != null)
            {
                // Xóa avatar cũ
                if (!string.IsNullOrEmpty(existing.AvatarUrl))
                {
                    await _firebase.DeleteFileAsync(existing.AvatarUrl);
                }

                // Upload avatar mới
                existing.AvatarUrl = await _firebase.UploadFileAsync(dto.Avatar, "doctors");
            }

            // Update profile fields
            existing.SpecialtyId = dto.SpecialtyId;
            existing.ExperienceYears = dto.ExperienceYears;
            existing.Description = dto.Description;
            existing.Workplace = dto.Workplace;
            existing.ConsultationFee = dto.ConsultationFee;

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
            bool hasActiveBookings = await _context.Bookings.AnyAsync(
                b => b.DoctorId == id && b.Status != BookingStatus.Cancelled);

            if (hasActiveBookings)
                throw new InvalidOperationException("Cannot delete doctor with active bookings.");

            // Xóa avatar từ Firebase
            if (!string.IsNullOrEmpty(existing.AvatarUrl))
            {
                await _firebase.DeleteFileAsync(existing.AvatarUrl);
            }

            await _repo.DeleteAsync(existing);
            _logger.LogInformation("Deleted doctor profile {Id}", id);
        }
    }
}
