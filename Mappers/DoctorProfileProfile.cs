using AutoMapper;
using BookingApp.DTOs.DoctorProfile;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class DoctorProfileProfile : Profile
    {
        public DoctorProfileProfile()
        {
            CreateMap<DoctorProfileCreateDto, DoctorProfile>().ReverseMap();
            CreateMap<DoctorProfileUpdateDto, DoctorProfile>().ReverseMap();

            CreateMap<DoctorProfile, DoctorProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.SpecialtyName, opt => opt.MapFrom(src => src.Specialty.Name))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.User.DateJoined))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber));

            CreateMap<DoctorProfile, DoctorProfileWithDetailsDto>()
               .IncludeBase<DoctorProfile, DoctorProfileDto>()
               .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                   src.Feedbacks.Any() ? src.Feedbacks.Average(f => f.Rating) : 0))
               .ForMember(dest => dest.TotalFeedbacks, opt => opt.MapFrom(src => src.Feedbacks.Count))
               .ForMember(dest => dest.AvailableSchedules, opt => opt.MapFrom(src =>
                   src.Schedules
                       .Where(s => s.IsAvailable)
                       .Select(s => new DoctorScheduleDto
                       {
                           ScheduleId = s.Id,
                           Date = s.Date,
                           StartTime = s.StartTime,
                           EndTime = s.EndTime
                       })));
        }
    }
}
