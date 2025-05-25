namespace SkillSyncAPI.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; } 

        public bool IsRead { get; set; } = false;

        // Foreign Key
        public int UserId { get; set; } // Recipient

        public ApplicationUser User { get; set; }
    }
}
