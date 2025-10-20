using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.Feedback;
using BookingApp.DTOs.Notification;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<FeedbackService> _logger;
        private readonly INotificationService _notificationService;

        public FeedbackService(
            IFeedbackRepository repo,
            ApplicationDbContext context, 
            IMapper mapper, 
            ILogger<FeedbackService> logger,
            INotificationService notificationService
            )
        {
            _repo = repo;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get all feedbacks với pagination và sorting (Admin only)
        /// </summary>
        public async Task<(IEnumerable<FeedbackDto> Feedbacks, int TotalCount)> GetAllAsync(
            int skip,
            int take,
            string sortBy,
            string sortOrder,
            int? doctorId = null)
        {
            var query = _context.Feedbacks
                .Include(f => f.PatientProfile)
                    .ThenInclude(p => p.User)
                .Include(f => f.DoctorProfile)
                    .ThenInclude(d => d.User)
                .AsQueryable();

            // Filter by doctor
            if (doctorId.HasValue)
            {
                query = query.Where(f => f.DoctorId == doctorId.Value);
            }

            var totalCount = await query.CountAsync();

            // Sorting
            query = sortBy.ToLower() switch
            {
                "rating" => sortOrder.ToUpper() == "DESC"
                    ? query.OrderByDescending(f => f.Rating)
                    : query.OrderBy(f => f.Rating),
                "createdat" => sortOrder.ToUpper() == "DESC"
                    ? query.OrderByDescending(f => f.CreatedAt)
                    : query.OrderBy(f => f.CreatedAt),
                "doctorid" => sortOrder.ToUpper() == "DESC"
                    ? query.OrderByDescending(f => f.DoctorId)
                    : query.OrderBy(f => f.DoctorId),
                "patientid" => sortOrder.ToUpper() == "DESC"
                    ? query.OrderByDescending(f => f.PatientId)
                    : query.OrderBy(f => f.PatientId),
                _ => sortOrder.ToUpper() == "DESC"
                    ? query.OrderByDescending(f => f.Id)
                    : query.OrderBy(f => f.Id)
            };

            var feedbacks = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);

            return (mapped, totalCount);
        }

        /// <summary>
        /// Get feedback by ID
        /// </summary>
        public async Task<FeedbackDto?> GetByIdAsync(int id)
        {
            var feedback = await _repo.GetByIdAsync(id);

            return feedback == null ? null : _mapper.Map<FeedbackDto>(feedback);
        }

        public async Task<(IEnumerable<FeedbackDto> Feedbacks, int TotalPages)> GetByDoctorIdAsync(int doctorId, int page, int pageSize, int? currentUserId)
        {
            var totalCount = await _repo.CountByDoctorIdAsync(doctorId);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var feedbacks = await _repo.GetByDoctorIdAsync(doctorId, page, pageSize);
            var mapped = _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);

            // Gắn flag CanEdit
            if (currentUserId != null)
            {
                foreach (var fb in mapped)
                {
                    fb.CanEdit = feedbacks.Any(f =>
                        f.Id == fb.Id &&
                        f.PatientProfile.UserId == currentUserId);
                }
            }

            return (mapped, totalPages);
        }

        public async Task<FeedbackDto> UpdateAsync(int id, FeedbackUpdateDto dto, int patientUserId)
        {
            var feedback = await _repo.GetByIdAsync(id);
            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found.");

            if (feedback.PatientProfile.UserId != patientUserId)
                throw new UnauthorizedAccessException("You can only edit your own feedback.");

            feedback.Rating = dto.Rating;
            feedback.Comment = dto.Comment;
            feedback.CreatedAt = DateTime.UtcNow; // mark updated

            await _repo.UpdateAsync(feedback);
            await UpdateDoctorRatingAsync(feedback.DoctorId);

            return _mapper.Map<FeedbackDto>(feedback);
        }

        public async Task<FeedbackDto> CreateAsync(FeedbackCreateDto dto, int patientUserId)
        {
            // Kiểm tra booking tồn tại và đã hoàn thành
            var booking = await _context.Bookings
                .Include(b => b.DoctorProfile)
                .Include(b => b.PatientProfile)
                .FirstOrDefaultAsync(b => b.Id == dto.BookingId);

            if (booking == null)
                throw new InvalidOperationException("Booking not found.");
            if (booking.Status != BookingStatus.Completed)
                throw new InvalidOperationException("You can only rate after the appointment is completed.");
            if (booking.PatientProfile.UserId != patientUserId)
                throw new UnauthorizedAccessException("You can only rate your own bookings.");

            // Kiểm tra đã feedback trước đó chưa
            bool alreadyFeedback = await _context.Feedbacks.AnyAsync(f =>
                f.DoctorId == booking.DoctorId && f.PatientId == booking.PatientId);

            if (alreadyFeedback)
                throw new InvalidOperationException("You have already submitted feedback for this booking.");

            // Tạo feedback
            var feedback = new Feedback
            {
                DoctorId = booking.DoctorId,
                PatientId = booking.PatientId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            await _repo.AddAsync(feedback);
            _logger.LogInformation("Feedback created for doctor {DoctorId} by patient {PatientId}", booking.DoctorId, booking.PatientId);

            // ⭐ Update doctor rating
            await UpdateDoctorRatingAsync(feedback.DoctorId);

            await _notificationService.CreateAsync(new NotificationCreateDto
            {
                UserId = booking.DoctorProfile.UserId,
                Message = $"Bạn nhận được phản hồi mới từ bệnh nhân {booking.PatientProfile.User.FullName}.",
            });

            return _mapper.Map<FeedbackDto>(feedback);
        }

        public async Task DeleteAsync(int id, int patientUserId, bool isAdmin = false)
        {
            var feedback = await _repo.GetByIdAsync(id);
            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found.");

            if (!isAdmin && feedback.PatientProfile.UserId != patientUserId)
                throw new UnauthorizedAccessException("You can only delete your own feedback.");

            await _repo.DeleteAsync(feedback);
            await UpdateDoctorRatingAsync(feedback.DoctorId);
        }

        // Cập nhật cache rating trong DoctorProfile
        private async Task UpdateDoctorRatingAsync(int doctorId)
        {
            var doctor = await _context.DoctorProfiles
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null) return;

            var feedbackStats = await _context.Feedbacks
               .Where(f => f.DoctorId == doctorId)
               .GroupBy(f => f.DoctorId)
               .Select(g => new
               {
                   Total = g.Count(),
                   Avg = g.Average(f => f.Rating)
               }).FirstOrDefaultAsync();

            doctor.TotalFeedbacks = feedbackStats?.Total ?? 0;
            doctor.AverageRating = feedbackStats?.Avg ?? 0;

            await _context.SaveChangesAsync();
        }
    }
}
