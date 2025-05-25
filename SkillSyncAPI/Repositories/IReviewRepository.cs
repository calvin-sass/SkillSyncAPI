using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories
{
    public interface IReviewRepository
    {
        IQueryable<Review> Query();

        void Add(Review review);

        void Update(Review review);

        void Delete(Review review);

        Task<Review?> GetByIdAsync(int id);

        Task SaveChangesAsync();
    }
}
