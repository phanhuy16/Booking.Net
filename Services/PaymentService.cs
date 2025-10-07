using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.Notification;
using BookingApp.DTOs.Payment;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public PaymentService(
            IPaymentRepository repo,
            ApplicationDbContext context,
            IMapper mapper,
            INotificationService notificationService)
        {
            _repo = repo;
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllAsync()
        {
            var payments = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<PaymentDto?> GetByIdAsync(int id)
        {
            var payment = await _repo.GetByIdAsync(id);
            return payment == null ? null : _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto?> GetByBookingIdAsync(int bookingId)
        {
            var payment = await _repo.GetByBookingIdAsync(bookingId);
            return payment == null ? null : _mapper.Map<PaymentDto>(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetByPatientAsync(int patientId)
        {
            var payments = await _repo.GetByPatientIdAsync(patientId);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<PaymentDto> CreateAsync(PaymentCreateDto dto)
        {
            var payment = _mapper.Map<Payment>(dto);
            payment.PaidAt = DateTime.UtcNow;

            await _repo.AddAsync(payment);
            await _repo.SaveAsync();

            // ✅ Sau khi thêm Payment mới, đồng bộ Booking nếu trạng thái = Completed
            await SyncBookingStatusAsync(payment.Id);

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<bool> UpdateStatusAsync(int id, PaymentUpdateDto dto)
        {
            var payment = await _repo.GetByIdAsync(id);
            if (payment == null) return false;

            payment.Status = dto.Status;

            if (dto.Status == PaymentStatus.Completed)
                payment.PaidAt = DateTime.UtcNow;

            await _repo.UpdateAsync(payment);
            await _repo.SaveAsync();

            // ✅ Khi thanh toán hoàn tất, tự động cập nhật booking
            await SyncBookingStatusAsync(payment.Id);

             // 🔔 Gửi thông báo cho bệnh nhân
            var booking = await _context.Bookings
                .Include(b => b.PatientProfile)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId);

            if (booking?.PatientProfile != null)
            {
                string msg = dto.Status switch
                {
                    PaymentStatus.Completed => "Thanh toán của bạn đã thành công!",
                    PaymentStatus.Failed => "Thanh toán của bạn thất bại. Vui lòng thử lại!",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(msg))
                {
                    await _notificationService.CreateAsync(new NotificationCreateDto
                    {
                        UserId = booking.PatientProfile.UserId,
                        Message = msg
                    });
                }
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _repo.GetByIdAsync(id);
            if (payment == null) return false;

            await _repo.DeleteAsync(payment);
            return true;
        }

        // ✅ Tính năng mới: đồng bộ ngược với Booking
        public async Task<bool> SyncBookingStatusAsync(int paymentId)
        {
            var payment = await _repo.GetByIdAsync(paymentId);
            if (payment == null) return false;

            var booking = await _context.Bookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId);

            if (booking == null) return false;

            // 🔄 Khi Payment.Completed → Booking.Completed
            if (payment.Status == PaymentStatus.Completed && booking.Status != BookingStatus.Completed)
            {
                booking.Status = BookingStatus.Completed;
                await _context.SaveChangesAsync();
            }

            // 🔄 Khi Payment.Failed → Booking.Cancelled (và mở lại lịch)
            if (payment.Status == PaymentStatus.Failed && booking.Status != BookingStatus.Cancelled)
            {
                booking.Status = BookingStatus.Cancelled;

                var schedule = await _context.Schedules.FindAsync(booking.ScheduleId);
                if (schedule != null)
                {
                    schedule.IsAvailable = true;
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
