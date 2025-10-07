using AutoMapper;
using BookingApp.DTOs.Notification;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationDto>();
            CreateMap<NotificationCreateDto, Notification>();
        }
    }
}
