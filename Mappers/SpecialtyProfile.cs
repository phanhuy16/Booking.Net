using AutoMapper;
using BookingApp.DTOs.Specialty;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class SpecialtyProfile : Profile
    {
        public SpecialtyProfile()
        {
            CreateMap<Specialty, SpecialtyDto>()
                .ForMember(dest => dest.DoctorCount, opt => opt.MapFrom(src => src.DoctorProfiles.Count));

            CreateMap<SpecialtyCreateDto, Specialty>();
            CreateMap<SpecialtyUpdateDto, Specialty>();
        }
    }
}
