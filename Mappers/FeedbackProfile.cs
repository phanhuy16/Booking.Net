using AutoMapper;
using BookingApp.DTOs.Feedback;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            CreateMap<Feedback, FeedbackDto>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.DoctorProfile.User.FullName))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.PatientProfile.User.FullName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.CanEdit, opt => opt.Ignore());

            CreateMap<FeedbackCreateDto, Feedback>();
        }
    }
}
