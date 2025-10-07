using AutoMapper;
using BookingApp.DTOs.MedicalRecord;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class MedicalRecordProfile : Profile
    {
        public MedicalRecordProfile()
        {
            CreateMap<MedicalRecord, MedicalRecordDto>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.DoctorProfile.User.FullName))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.PatientProfile.User.FullName));

            CreateMap<MedicalRecordCreateDto, MedicalRecord>();
        }
    }
}
