using AutoMapper;
using BookingApp.DTOs.Payment;
using BookingApp.Models;

namespace BookingApp.Mappers
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, PaymentDto>();
            CreateMap<PaymentCreateDto, Payment>();
        }
    }
}
