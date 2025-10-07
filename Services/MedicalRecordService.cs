using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.MedicalRecord;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace BookingApp.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository _repo;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicalRecordService> _logger;
        private readonly FirebaseStorageService _firebase;

        public MedicalRecordService(
            IMedicalRecordRepository repo, 
            ApplicationDbContext context, 
            IMapper mapper,
            ILogger<MedicalRecordService> logger,
            FirebaseStorageService firebase
            )
        {
            _repo = repo;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _firebase = firebase;
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetAllAsync()
        {
            var records = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<MedicalRecordDto>>(records);
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetByPatientAsync(int patientUserId)
        {
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == patientUserId);
            if (patient == null) throw new UnauthorizedAccessException("Patient profile not found.");

            var records = await _repo.GetByPatientIdAsync(patient.Id);
            return _mapper.Map<IEnumerable<MedicalRecordDto>>(records);
        }

        public async Task<MedicalRecordDto?> GetByIdAsync(int id, int currentUserId, string role)
        {
            var record = await _repo.GetByIdAsync(id);
            if (record == null) return null;

            // Check permission
            if (role == "Patient" && record.PatientProfile.UserId != currentUserId)
                throw new UnauthorizedAccessException("You can only view your own medical records.");

            return _mapper.Map<MedicalRecordDto>(record);
        }

        public async Task<MedicalRecordDto> CreateAsync(MedicalRecordCreateDto dto, int doctorUserId, IFormFile? attachment)
        {
            var booking = await _context.Bookings
                .Include(b => b.PatientProfile)
                .Include(b => b.DoctorProfile)
                .FirstOrDefaultAsync(b => b.Id == dto.BookingId);

            if (booking == null)
                throw new InvalidOperationException("Booking not found.");
            if (booking.Status != BookingStatus.Completed)
                throw new InvalidOperationException("Medical record can only be created for completed bookings.");
            if (booking.DoctorProfile.UserId != doctorUserId)
                throw new UnauthorizedAccessException("You can only create records for your own patients.");

            // 🔥 Upload file (nếu có)
            string? fileUrl = null;
            if (attachment != null)
                fileUrl = await _firebase.UploadFileAsync(attachment, "medical-records");

            var record = new MedicalRecord
            {
                PatientId = booking.PatientId,
                DoctorId = booking.DoctorId,
                BookingId = booking.Id,
                Diagnosis = dto.Diagnosis,
                Prescription = dto.Prescription,
                Notes = dto.Notes,
                AttachmentUrl = fileUrl
            };

            await _repo.AddAsync(record);
            _logger.LogInformation("Medical record created for patient {PatientId} by doctor {DoctorId}", record.PatientId, record.DoctorId);

            return _mapper.Map<MedicalRecordDto>(record);
        }

        // 🧹 Tự động xóa file trên Firebase nếu record bị xóa
        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _repo.GetByIdAsync(id);
            if (record == null) return false;

            if (!string.IsNullOrEmpty(record.AttachmentUrl))
                await _firebase.DeleteFileAsync(record.AttachmentUrl);

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
