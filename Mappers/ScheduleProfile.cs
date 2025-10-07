using AutoMapper;
using BookingApp.DTOs.Schedule;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class ScheduleProfile : Profile
    {
        public ScheduleProfile()
        {
            CreateMap<ScheduleCreateDto, Schedule>();
            CreateMap<ScheduleUpdateDto, Schedule>();
            CreateMap<Schedule, ScheduleDto>()
                .ForMember(dest => dest.DoctorName,
                           opt => opt.MapFrom(src => src.DoctorProfile.User.FullName))
                .ForMember(dest => dest.TotalBookings,
                           opt => opt.MapFrom(src => src.Bookings.Count));
        }
    }
}
