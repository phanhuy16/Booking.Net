using AutoMapper;
using BookingApp.DTOs.Booking;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.ServicePrice, opt => opt.MapFrom(src => src.Service.Price))
                .ForMember(dest => dest.ServiceTitle, opt => opt.MapFrom(src => src.Service.Title));

            CreateMap<BookingCreateDto, Booking>();
        }
    }
}
