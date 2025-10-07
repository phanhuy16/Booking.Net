using AutoMapper;
using BookingApp.Data;
using BookingApp.DTOs.Service;
using BookingApp.Interface.IRepository;
using BookingApp.Interface.IService;
using BookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceManager> _logger;

        public ServiceManager(IServiceRepository serviceRepository, ApplicationDbContext context, IMapper mapper, ILogger<ServiceManager> logger)
        {
            _serviceRepository = serviceRepository;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceDto>> GetAllServicesAsync()
        {
            _logger.LogInformation("Fetching all services from DB.");
            var services = await _serviceRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceDto>>(services);
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(int id)
        {
            _logger.LogInformation("Fetching service with ID {Id}", id);
            var service = await _serviceRepository.GetByIdAsync(id);
            return service == null ? null : _mapper.Map<ServiceDto>(service);
        }

        public async Task<ServiceDto> AddServiceAsync(ServiceCreateDto dto)
        {
            // Kiểm tra trùng tên
            if (await _context.Services.AnyAsync(s => s.Title.ToLower() == dto.Title.ToLower()))
                throw new InvalidOperationException("A service with the same title already exists.");

            var entity = _mapper.Map<Service>(dto);
            await _serviceRepository.AddAsync(entity);
            _logger.LogInformation("Added new service: {Title}", entity.Title);

            return _mapper.Map<ServiceDto>(entity);
        }

        public async Task<ServiceDto> UpdateServiceAsync(int id, ServiceUpdateDto dto)
        {
            var existing = await _serviceRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Service not found.");

            if (await _context.Services.AnyAsync(s => s.Title.ToLower() == dto.Title.ToLower() && s.Id != id))
                throw new InvalidOperationException("Another service with the same title already exists.");

            // Map các field từ dto sang entity
            _mapper.Map(dto, existing);
            await _serviceRepository.UpdateAsync(existing);

            _logger.LogInformation("Updated service {Id}", id);
            return _mapper.Map<ServiceDto>(existing);
        }

        public async Task DeleteServiceAsync(int id)
        {
            var existing = await _serviceRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Service not found.");

            // Kiểm tra xem có booking nào đang sử dụng không
            bool hasActiveBookings = await _context.Bookings.AnyAsync(b => b.ServiceId == id && b.Status != BookingStatus.Cancelled);
            if (hasActiveBookings)
                throw new InvalidOperationException("Cannot delete a service with active bookings.");

            await _serviceRepository.DeleteAsync(id);
            _logger.LogInformation("Deleted service {Id}", id);
        }
    }
}
