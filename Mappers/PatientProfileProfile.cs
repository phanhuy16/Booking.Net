using AutoMapper;
using BookingApp.DTOs.PatientProfile;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class PatientProfileProfile : Profile
    {
        public PatientProfileProfile()
        {
            // Create & Update
            CreateMap<PatientProfileCreateDto, PatientProfile>();
            CreateMap<PatientProfileUpdateDto, PatientProfile>();

            // Read
            CreateMap<PatientProfile, PatientProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()));

            CreateMap<PatientProfile, PatientProfileWithDetailsDto>()
                .IncludeBase<PatientProfile, PatientProfileDto>()
                .ForMember(dest => dest.Bookings, opt => opt.MapFrom(src => src.Bookings))
                .ForMember(dest => dest.MedicalRecords, opt => opt.MapFrom(src => src.MedicalRecords));

            CreateMap<Booking, PatientBookingDto>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.DoctorProfile.User.FullName))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<MedicalRecord, PatientMedicalRecordDto>();
        }
    }
}
