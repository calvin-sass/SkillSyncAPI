using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories.Impl
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context) => _context = context;

        public IQueryable<Review> Query() => _context.Reviews;

        public void Add(Review review) => _context.Reviews.Add(review);

        public void Update(Review review) => _context.Reviews.Update(review);

        public void Delete(Review review) => _context.Reviews.Remove(review);

        public async Task<Review?> GetByIdAsync(int id) => await _context.Reviews.FindAsync(id);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
