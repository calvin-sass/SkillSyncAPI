using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories
{
    public interface INotificationRepository
    {
        IQueryable<Notification> Query();

        Notification? GetById(int id);

        void Add(Notification notification);

        void Update(Notification notification);

        void Delete(Notification notification);

        Task<int> SaveChangesAsync();
    }
}
