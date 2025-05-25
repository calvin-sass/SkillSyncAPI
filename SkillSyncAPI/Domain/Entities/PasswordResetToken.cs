namespace SkillSyncAPI.Domain.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
