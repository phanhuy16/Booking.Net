using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.Feedback;
using BookingApp.DTOs.PatientProfile;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookingApp.Services
{
    public class PatientProfileService : IPatientProfileService
    {
        private readonly IPatientProfileRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<PatientProfileService> _logger;
        private readonly ApplicationDbContext _context;

        public PatientProfileService(IPatientProfileRepository repo,
            IMapper mapper,
            ILogger<PatientProfileService> logger,
            ApplicationDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public async Task<(IEnumerable<PatientProfileDto> PatientProfiles, int TotalCount)> GetAllAsync(int skip,
           int take,
           string sortBy,
           string sortOrder)
        {
            var query = _repo.GetAllAsync(includeDetails: true); // Bỏ await

            // Đếm tổng số TRƯỚC khi phân trang
            var totalCount = await query.CountAsync();

            // Sorting TRƯỚC KHI map - sort trên PatientProfile entity
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortOrder.ToUpper() == "DESC"
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id);
            }

            // Pagination
            var patients = await query.Skip(skip).Take(take).ToListAsync();
            var mapped = _mapper.Map<IEnumerable<PatientProfileDto>>(patients);

            return (mapped, totalCount);
        }

        public async Task<PatientProfileWithDetailsDto?> GetByIdAsync(int id)
        {
            var patient = await _repo.GetByIdAsync(id, includeDetails: true);
            return patient == null ? null : _mapper.Map<PatientProfileWithDetailsDto>(patient);
        }

        public async Task<PatientDashboardDto?> GetDashboardAsync(int patientId)
        {
            var patient = await _repo.GetByIdAsync(patientId, includeDetails: true);
            if (patient == null)
                return null;

            var dashboard = new PatientDashboardDto
            {
                PatientId = patient.Id,
                PatientName = patient.User.FullName,
                TotalBookings = patient.Bookings.Count,
                CompletedBookings = patient.Bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = patient.Bookings.Count(b => b.Status == BookingStatus.Cancelled),
                TotalMedicalRecords = patient.MedicalRecords.Count,
                UpcomingBookings = patient.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed && b.Schedule.Date >= DateTime.UtcNow.Date)
                    .Select(b => new UpcomingBookingDto
                    {
                        BookingId = b.Id,
                        DoctorName = b.DoctorProfile.User.FullName,
                        ServiceName = b.Service.Title,
                        Date = b.Schedule.Date
                    }).OrderBy(b => b.Date).Take(5).ToList()
            };

            // Feedback mới nhất
            var latestFeedback = await _context.Feedbacks
                .Where(f => f.PatientId == patient.Id)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new LatestFeedbackDto
                {
                    DoctorName = f.DoctorProfile.User.FullName,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                })
                .FirstOrDefaultAsync();

            dashboard.LatestFeedback = latestFeedback;
            return dashboard;
        }

        public async Task<PatientStatisticsDto?> GetStatisticsAsync(int patientId, DateTime from, DateTime to)
        {
            var patient = await _repo.GetByIdAsync(patientId);
            if (patient == null)
                return null;

            var patientName = patient.User.FullName;

            // Lấy danh sách Booking trong khoảng thời gian
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Schedule)
                .Where(b => b.PatientId == patientId &&
                            b.Schedule.Date >= from.Date &&
                            b.Schedule.Date <= to.Date)
                .ToListAsync();

            // Lấy danh sách Feedback trong khoảng thời gian
            var feedbacks = await _context.Feedbacks
                .Include(f => f.DoctorProfile)
                .Where(f => f.PatientId == patientId &&
                            f.CreatedAt >= from &&
                            f.CreatedAt <= to)
                .ToListAsync();

            // Gom nhóm theo tháng
            var grouped = bookings
                .GroupBy(b => new { b.Schedule.Date.Year, b.Schedule.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new PatientMonthlyStatisticDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalBookings = g.Count(),
                    Completed = g.Count(b => b.Status == BookingStatus.Completed),
                    Cancelled = g.Count(b => b.Status == BookingStatus.Cancelled),
                    AverageRating = feedbacks
                        .Where(f => f.CreatedAt.Year == g.Key.Year && f.CreatedAt.Month == g.Key.Month)
                        .Select(f => (double?)f.Rating)
                        .DefaultIfEmpty(null)
                        .Average()
                }).ToList();

            return new PatientStatisticsDto
            {
                PatientId = patientId,
                PatientName = patientName,
                Statistics = grouped
            };
        }

        public async Task<PatientProfileDto> CreateAsync(PatientProfileCreateDto dto)
        {
            var entity = _mapper.Map<PatientProfile>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<PatientProfileDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, PatientProfileUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return false;

            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return false;

            await _repo.DeleteAsync(entity);
            return true;
        }
    }
}
