using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories
{
    public interface IPaymentRepository
    {
        IQueryable<Payment> Query();

        Payment? GetById(int id);

        void Add(Payment payment);

        void Update(Payment payment);

        Task<int> SaveChangesAsync();
    }
}
