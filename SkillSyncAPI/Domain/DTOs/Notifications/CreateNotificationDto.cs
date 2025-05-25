namespace SkillSyncAPI.Domain.DTOs.Notifications
{
    public class CreateNotificationDto
    {
        public int UserId { get; set; }

        public string Message { get; set; }
    }
}
