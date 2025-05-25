using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories.Impl
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Notification> Query() => _context.Notifications;

        public Notification? GetById(int id) => _context.Notifications.Find(id);

        public void Add(Notification notification) => _context.Notifications.Add(notification);

        public void Update(Notification notification) => _context.Notifications.Update(notification);

        public void Delete(Notification notification) => _context.Notifications.Remove(notification);

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
