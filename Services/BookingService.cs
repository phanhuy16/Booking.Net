using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.Booking;
using BookingApp.DTOs.Notification;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace BookingApp.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public BookingService(
           IBookingRepository bookingRepository,
           IMapper mapper,
           ApplicationDbContext context,
           INotificationService notificationService)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<BookingDto> CreateAsync(BookingCreateDto dto)
        {
            var schedule = await _context.Schedules.FindAsync(dto.ScheduleId);
            if (schedule == null || !schedule.IsAvailable)
                throw new InvalidOperationException("Lịch khám không khả dụng.");

            var service = await _context.Services.FindAsync(dto.ServiceId);
            if (service == null)
                throw new InvalidOperationException("Dịch vụ không tồn tại.");

            var booking = _mapper.Map<Booking>(dto);
            booking.Status = BookingStatus.Pending;
            booking.CreatedAt = DateTime.UtcNow;

            await _bookingRepository.AddAsync(booking);
            schedule.IsAvailable = false;

            await _bookingRepository.SaveAsync();

            // 🔔 Gửi thông báo cho bác sĩ
            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == booking.DoctorId);

            if (doctor != null)
            {
                await _notificationService.CreateAsync(new NotificationCreateDto
                {
                    UserId = doctor.UserId,
                    Message = $"Bạn có một lịch hẹn mới từ bệnh nhân ID {booking.PatientId}."
                });
            }

            return _mapper.Map<BookingDto>(booking);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return false;

            await _bookingRepository.DeleteAsync(booking);
            await _bookingRepository.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<BookingDto>> GetAllAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }

        public async Task<BookingDto?> GetByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            return booking == null ? null : _mapper.Map<BookingDto>(booking);
        }

        public async Task<bool> UpdateStatusAsync(int id, BookingUpdateDto dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return false;

            booking.Status = dto.Status;

             var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == booking.DoctorId);

            var patient = await _context.PatientProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == booking.PatientId);

            // Khi Confirmed => tạo Payment nếu chưa có
            if (dto.Status == BookingStatus.Confirmed)
            {
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.BookingId == booking.Id);

                if (existingPayment == null)
                {
                    var payment = new Payment
                    {
                        BookingId = booking.Id,
                        Amount = booking.Service.Price,
                        Method = PaymentMethod.Online,
                        Status = PaymentStatus.Pending
                    };
                    _context.Payments.Add(payment);

                    if (patient != null)
                    {
                        await _notificationService.CreateAsync(new NotificationCreateDto
                        {
                            UserId = patient.UserId,
                            Message = $"Lịch hẹn của bạn với bác sĩ {doctor?.User.FullName} đã được xác nhận."
                        });
                    }
                }
            }

            // ✅ Khi Completed => cập nhật Payment thành Completed
            if (dto.Status == BookingStatus.Completed)
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.BookingId == booking.Id);

                if (payment != null && payment.Status != PaymentStatus.Completed)
                {
                    payment.Status = PaymentStatus.Completed;
                    payment.PaidAt = DateTime.UtcNow;
                }

                await _notificationService.CreateAsync(new NotificationCreateDto
                {
                    UserId = patient.UserId,
                    Message = $"Cuộc hẹn với bác sĩ {doctor?.User.FullName} đã hoàn tất. Cảm ơn bạn đã sử dụng dịch vụ!"
                });
            }

            // ✅ Khi Cancelled => mở lại lịch
            if (dto.Status == BookingStatus.Cancelled)
            {
                var schedule = await _context.Schedules.FindAsync(booking.ScheduleId);
                if (schedule != null)
                {
                    schedule.IsAvailable = true;
                }

                // Nếu đã có Payment, thì đánh dấu thất bại
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.BookingId == booking.Id);
                if (payment != null)
                {
                    payment.Status = PaymentStatus.Failed;
                }

                if (patient != null)
                {
                    await _notificationService.CreateAsync(new NotificationCreateDto
                    {
                        UserId = patient.UserId,
                        Message = $"Lịch hẹn của bạn đã bị hủy."
                    });
                }
            }

            await _bookingRepository.SaveAsync();

            return true;
        }
    }
}
