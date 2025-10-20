using BookingApp.DTOs.Feedback;

namespace BookingApp.Interface.IService
{
    public interface IFeedbackService
    {
        Task<(IEnumerable<FeedbackDto> Feedbacks, int TotalCount)> GetAllAsync(
            int skip,
            int take,
            string sortBy,
            string sortOrder,
            int? doctorId = null);

        Task<FeedbackDto?> GetByIdAsync(int id);
        Task<(IEnumerable<FeedbackDto> Feedbacks, int TotalPages)> GetByDoctorIdAsync(int doctorId, int page, int pageSize, int? currentUserId);
        Task<FeedbackDto> UpdateAsync(int id, FeedbackUpdateDto dto, int patientUserId);
        Task<FeedbackDto> CreateAsync(FeedbackCreateDto dto, int patientUserId);
        Task DeleteAsync(int id, int patientUserId, bool isAdmin = false);
    }
}
