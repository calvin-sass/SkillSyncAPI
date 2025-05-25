using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories
{
    public interface IServiceRepository
    {
        IQueryable<Service> Query(); // For flexible queries (filtering, includes)

        Service? GetById(int id);

        void Add(Service service);

        void Update(Service service);

        void Delete(Service service);

        Task SaveChangesAsync();
    }
}
