using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<ApplicationUser> GetAll();

        ApplicationUser? GetById(int id);

        void Add(ApplicationUser user);

        void Update(ApplicationUser user);

        void Delete(ApplicationUser user);

        Task SaveChangesAsync();
    }
}
