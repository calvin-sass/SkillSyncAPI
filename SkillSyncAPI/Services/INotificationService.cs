using SkillSyncAPI.Domain.DTOs.Notifications;

namespace SkillSyncAPI.Services
{
    public interface INotificationService
    {
        Task SendAsync(int userId, string message);

        Task MarkAsReadAsync(int notificationId, int userId);

        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId);
    }
}
