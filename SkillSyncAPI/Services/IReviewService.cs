using SkillSyncAPI.Domain.DTOs.Reviews;

namespace SkillSyncAPI.Services
{
    public interface IReviewService
    {
        Task<bool> CanUserReviewServiceAsync(int userId, int serviceId);

        Task<ReviewDto?> CreateReviewAsync(int userId, ReviewCreateDto dto);

        Task<bool> UpdateReviewAsync(int userId, int reviewId, ReviewUpdateDto dto);

        Task<bool> DeleteReviewAsync(int userId, int reviewId);

        Task<List<ReviewDto>> GetReviewsForServiceAsync(int serviceId);

        Task<List<ReviewDto>> GetReviewsByUserAsync(int userId);
    }
}
