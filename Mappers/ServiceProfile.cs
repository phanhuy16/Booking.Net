using AutoMapper;
using BookingApp.DTOs.Service;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<Service, ServiceDto>();
            CreateMap<ServiceCreateDto, Service>();
            CreateMap<ServiceUpdateDto, Service>();
        }
    }
}
