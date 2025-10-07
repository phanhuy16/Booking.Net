using BookingApp.DTOs.DoctorProfile;
using BookingApp.DTOs.Specialty;

namespace BookingApp.Interface.IService
{
    public interface ISpecialtyService
    {
        Task<IEnumerable<SpecialtyDto>> GetAllAsync();
        Task<SpecialtyDto?> GetByIdAsync(int id);
        Task<SpecialtyDto> CreateAsync(SpecialtyCreateDto dto);
        Task<SpecialtyDto?> UpdateAsync(int id, SpecialtyUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<(IEnumerable<DoctorInSpecialtyDto> Doctors, int TotalPages)>
          GetDoctorsBySpecialtyIdAsync(int specialtyId,
                                       int page,
                                       int pageSize,
                                       string sortBy,
                                       string order,
                                       double? minRating,
                                       int? minExperience);
    }
}
