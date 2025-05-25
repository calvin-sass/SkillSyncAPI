namespace SkillSyncAPI.Domain.DTOs.Users
{
    public class UserProfileDto
    {
        public string? AvatarUrl { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
