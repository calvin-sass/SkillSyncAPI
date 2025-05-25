namespace SkillSyncAPI.Domain.Entities
{
    public class PendingUser
    {
        public int Id { get; set; }

        public string Email { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string Role { get; set; }

        public string Code { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
