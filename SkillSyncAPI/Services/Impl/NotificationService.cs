using SkillSyncAPI.Domain.DTOs.Notifications;
using SkillSyncAPI.Domain.Entities;
using SkillSyncAPI.Repositories;

namespace SkillSyncAPI.Services.Impl
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;

        public NotificationService(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task SendAsync(int userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _notificationRepo.Add(notification);
            await _notificationRepo.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = _notificationRepo.GetById(notificationId);
            if (notification == null || notification.UserId != userId)
                return;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;
                _notificationRepo.Update(notification);
                await _notificationRepo.SaveChangesAsync();
            }
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            // Use ToListAsync if using EF Core, otherwise ToList()
            var notifications = _notificationRepo.Query()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToList();

            return await Task.FromResult(notifications);
        }
    }
}
