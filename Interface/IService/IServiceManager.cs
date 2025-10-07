using BookingApp.DTOs.Service;
using BookingApp.Models;

namespace BookingApp.Interface.IService
{
    public interface IServiceManager
    {
        Task<IEnumerable<ServiceDto>> GetAllServicesAsync();
        Task<ServiceDto?> GetServiceByIdAsync(int id);
        Task<ServiceDto> AddServiceAsync(ServiceCreateDto dto);
        Task<ServiceDto> UpdateServiceAsync(int id, ServiceUpdateDto dto);
        Task DeleteServiceAsync(int id);
    }
}
