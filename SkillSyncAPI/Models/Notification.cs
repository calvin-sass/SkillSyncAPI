using SkillSyncAPI.Helpers.Interfaces;

namespace SkillSyncAPI.Models
{
    public class Notification : IAuditable
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; } 

        public bool IsRead { get; set; } = false;

        // Foreign Key
        public int UserId { get; set; } // Recipient

        public User User { get; set; }
    }
}
